using BaZic.Runtime.BaZic.Code;
using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using BaZic.Runtime.BaZic.Code.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace BaZic.Runtime.Tests.BaZic.Code.Optimizer
{
    [TestClass]
    public class BaZicOptimizerTest
    {
        [TestMethod]
        public void BaZicOptimizerConditionInlining()
        {
            var parser = new BaZicParser();
            var codeGenerator = new BaZicCodeGenerator();

            var inputCode =
@"FUNCTION Method1()
    IF TRUE THEN
        VARIABLE foo = 1
        VARIABLE bar = 1
        IF foo = bar THEN
            RETURN foo
        END IF
    ELSE
        VARIABLE foo = 2
        VARIABLE bar = 3
    END IF
END FUNCTION";

            var program = parser.Parse(inputCode, true);
            var result = codeGenerator.Generate(program.Program);

            var expectedResult =
@"# BaZic code generated automatically

FUNCTION Method1()
    IF NOT TRUE GOTO _A
    VARIABLE foo = 1
    VARIABLE bar = 1
    IF NOT (foo = bar) GOTO _C
    RETURN foo
    _C:
    GOTO _B
    _A:
    VARIABLE foo = 2
    VARIABLE bar = 3
    _B:
END FUNCTION";

            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod]
        public void BaZicOptimizerIterationInlining()
        {
            var parser = new BaZicParser();
            var codeGenerator = new BaZicCodeGenerator();

            var inputCode =
@"FUNCTION Method1()
    DO WHILE TRUE
        DO
            BREAK
            VARIABLE x = 1
        LOOP WHILE FALSE
    LOOP
END FUNCTION";

            var program = parser.Parse(inputCode, true);
            var result = codeGenerator.Generate(program.Program);

            var expectedResult =
@"# BaZic code generated automatically

FUNCTION Method1()
    _A:
    IF NOT TRUE GOTO _B
    _C:
    GOTO _D
    VARIABLE x = 1
    IF NOT FALSE GOTO _D
    GOTO _C
    _D:
    GOTO _A
    _B:
END FUNCTION";

            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod]
        public void BaZicOptimizerMethodInliningStatement()
        {
            var parser = new BaZicParser();
            var codeGenerator = new BaZicCodeGenerator();

            var inputCode =
@"EXTERN FUNCTION Main(args[])
    Method1()
    Method1()
END FUNCTION

FUNCTION Method1()
    VARIABLE x = 1
    x = 2
    RETURN
END FUNCTION";

            var program = parser.Parse(inputCode, true);
            var result = codeGenerator.Generate(program.Program);

            var expectedResult =
@"# BaZic code generated automatically

EXTERN FUNCTION Main(args[])
    VARIABLE RET_A
    _A:
    VARIABLE x = 1
    x = 2
    GOTO _B
    _B:
    VARIABLE RET_C
    _C:
    VARIABLE x = 1
    x = 2
    GOTO _D
    _D:
END FUNCTION

FUNCTION Method1()
    VARIABLE x = 1
    x = 2
    RETURN 
END FUNCTION";

            var variable1Decl = (VariableDeclaration)program.Program.Methods.Last().Statements.First();
            var variable1Ref = (VariableReferenceExpression)((AssignStatement)program.Program.Methods.Last().Statements[1]).LeftExpression;
            var variable2Decl = (VariableDeclaration)program.Program.Methods.First().Statements[2];
            var variable2Ref = (VariableReferenceExpression)((AssignStatement)program.Program.Methods.First().Statements[3]).LeftExpression;
            var variable3Decl = (VariableDeclaration)program.Program.Methods.First().Statements[8];
            var variable3Ref = (VariableReferenceExpression)((AssignStatement)program.Program.Methods.First().Statements[9]).LeftExpression;

            Assert.AreEqual(variable1Decl.Id, variable1Ref.VariableDeclarationID);
            Assert.AreNotEqual(variable1Decl.Id, variable2Decl.Id);
            Assert.AreNotEqual(variable1Decl.Id, variable2Ref.VariableDeclarationID);
            Assert.AreEqual(variable2Decl.Id, variable2Ref.VariableDeclarationID);
            Assert.AreNotEqual(variable2Decl.Id, variable3Decl.Id);
            Assert.AreNotEqual(variable2Decl.Id, variable3Ref.VariableDeclarationID);
            Assert.AreEqual(variable3Decl.Id, variable3Ref.VariableDeclarationID);
            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod]
        public void BaZicOptimizerMethodInliningExpression()
        {
            var parser = new BaZicParser();
            var codeGenerator = new BaZicCodeGenerator();

            var inputCode =
@"FUNCTION Method1(arg1, arg2)
    RETURN arg1 + arg2
END FUNCTION

EXTERN FUNCTION Main(args[])
    Method1(1, 2)
    VARIABLE x = Method1(1, 2)
    VARIABLE y
    y = Method1(1, 2)
    RETURN Method1(1, 2)
END FUNCTION";

            var program = parser.Parse(inputCode, true);
            var result = codeGenerator.Generate(program.Program);

            var expectedResult =
@"# BaZic code generated automatically

FUNCTION Method1(arg1, arg2)
    RETURN arg1 + arg2
END FUNCTION

EXTERN FUNCTION Main(args[])
    VARIABLE arg1 = 1
    VARIABLE arg2 = 2
    VARIABLE RET_A
    _A:
    RET_A = arg1 + arg2
    GOTO _B
    _B:
    VARIABLE x
    VARIABLE arg1 = 1
    VARIABLE arg2 = 2
    VARIABLE RET_C
    _C:
    RET_C = arg1 + arg2
    GOTO _D
    _D:
    x = RET_C
    VARIABLE y
    VARIABLE arg1 = 1
    VARIABLE arg2 = 2
    VARIABLE RET_E
    _E:
    RET_E = arg1 + arg2
    GOTO _F
    _F:
    y = RET_E
    VARIABLE RET_G
    VARIABLE arg1 = 1
    VARIABLE arg2 = 2
    VARIABLE RET_H
    _H:
    RET_H = arg1 + arg2
    GOTO _I
    _I:
    RET_G = RET_H
    RETURN RET_G
END FUNCTION";

            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod]
        public void BaZicOptimizerAsyncMethodInlining()
        {
            var parser = new BaZicParser();
            var codeGenerator = new BaZicCodeGenerator();

            var inputCode =
@"FUNCTION Method1(arg1)
    VARIABLE x = 1
END FUNCTION

ASYNC FUNCTION Method2()
    VARIABLE x = 1
END FUNCTION

EXTERN FUNCTION Main(args[])
    Method1(Method1(NULL))
    Method2()
    AWAIT Method2()
END FUNCTION";

            var program = parser.Parse(inputCode, true);
            var result = codeGenerator.Generate(program.Program);

            var expectedResult =
@"# BaZic code generated automatically

FUNCTION Method1(arg1)
    VARIABLE x = 1
END FUNCTION

ASYNC FUNCTION Method2()
    VARIABLE x = 1
END FUNCTION

EXTERN FUNCTION Main(args[])
    VARIABLE arg1 = Method1(NULL)
    VARIABLE RET_A
    _A:
    VARIABLE x = 1
    _B:
    Method2()
    AWAIT Method2()
END FUNCTION";

            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod]
        public void BaZicOptimizerMethodInliningRecursvity()
        {
            var parser = new BaZicParser();
            var codeGenerator = new BaZicCodeGenerator();

            var inputCode =
@"EXTERN FUNCTION Main(args[])
    RETURN Method1(100)
END FUNCTION

FUNCTION Method1(num)
    IF num > 1 THEN
        RETURN Method1(num - 1)
    END IF

    RETURN num
END FUNCTION";

            var program = parser.Parse(inputCode, true).Program;
            var result = codeGenerator.Generate(program);

            var expectedResult =
@"# BaZic code generated automatically

EXTERN FUNCTION Main(args[])
    VARIABLE RET_A
    VARIABLE num = 100
    VARIABLE RET_B
    _B:
    IF NOT (num >= 1) GOTO _D
    VARIABLE num = num - 1
    VARIABLE RET_E
    _E:
    IF NOT (num >= 1) GOTO _G
    VARIABLE num = num - 1
    VARIABLE RET_H
    _H:
    IF NOT (num >= 1) GOTO _J
    VARIABLE num = num - 1
    VARIABLE RET_K
    _K:
    IF NOT (num >= 1) GOTO _M
    VARIABLE num = num - 1
    VARIABLE RET_N
    _N:
    IF NOT (num >= 1) GOTO _P
    VARIABLE num = num - 1
    VARIABLE RET_Q
    _Q:
    IF NOT (num >= 1) GOTO _S
    VARIABLE num = num - 1
    VARIABLE RET_T
    _T:
    IF NOT (num >= 1) GOTO _V
    VARIABLE num = num - 1
    VARIABLE RET_W
    _W:
    IF NOT (num >= 1) GOTO _Y
    VARIABLE num = num - 1
    VARIABLE RET_Z
    _Z:
    IF NOT (num >= 1) GOTO _AB
    VARIABLE num = num - 1
    VARIABLE RET_AC
    _AC:
    IF NOT (num >= 1) GOTO _AE
    VARIABLE num = num - 1
    VARIABLE RET_AF
    _AF:
    IF NOT (num >= 1) GOTO _AH
    VARIABLE num = num - 1
    VARIABLE RET_AI
    _AI:
    IF NOT (num >= 1) GOTO _AK
    VARIABLE num = num - 1
    VARIABLE RET_AL
    _AL:
    IF NOT (num >= 1) GOTO _AN
    VARIABLE num = num - 1
    VARIABLE RET_AO
    _AO:
    IF NOT (num >= 1) GOTO _AQ
    VARIABLE num = num - 1
    VARIABLE RET_AR
    _AR:
    IF NOT (num >= 1) GOTO _AT
    VARIABLE num = num - 1
    VARIABLE RET_AU
    _AU:
    IF NOT (num >= 1) GOTO _AW
    VARIABLE num = num - 1
    VARIABLE RET_AX
    _AX:
    IF NOT (num >= 1) GOTO _AZ
    VARIABLE num = num - 1
    VARIABLE RET_BA
    _BA:
    IF NOT (num >= 1) GOTO _BC
    VARIABLE num = num - 1
    VARIABLE RET_BD
    _BD:
    IF NOT (num >= 1) GOTO _BF
    VARIABLE num = num - 1
    VARIABLE RET_BG
    _BG:
    IF NOT (num >= 1) GOTO _BI
    VARIABLE num = num - 1
    VARIABLE RET_BJ
    _BJ:
    IF NOT (num >= 1) GOTO _BL
    VARIABLE num = num - 1
    VARIABLE RET_BM
    _BM:
    IF NOT (num >= 1) GOTO _BO
    VARIABLE num = num - 1
    VARIABLE RET_BP
    _BP:
    IF NOT (num >= 1) GOTO _BR
    VARIABLE num = num - 1
    VARIABLE RET_BS
    _BS:
    IF NOT (num >= 1) GOTO _BU
    VARIABLE num = num - 1
    VARIABLE RET_BV
    _BV:
    IF NOT (num >= 1) GOTO _BX
    VARIABLE num = num - 1
    VARIABLE RET_BY
    _BY:
    IF NOT (num >= 1) GOTO _CA
    VARIABLE num = num - 1
    VARIABLE RET_CB
    _CB:
    IF NOT (num >= 1) GOTO _CD
    VARIABLE num = num - 1
    VARIABLE RET_CE
    _CE:
    IF NOT (num >= 1) GOTO _CG
    VARIABLE num = num - 1
    VARIABLE RET_CH
    _CH:
    IF NOT (num >= 1) GOTO _CJ
    VARIABLE num = num - 1
    VARIABLE RET_CK
    _CK:
    IF NOT (num >= 1) GOTO _CM
    VARIABLE num = num - 1
    VARIABLE RET_CN
    _CN:
    IF NOT (num >= 1) GOTO _CP
    VARIABLE num = num - 1
    VARIABLE RET_CQ
    _CQ:
    IF NOT (num >= 1) GOTO _CS
    VARIABLE num = num - 1
    VARIABLE RET_CT
    _CT:
    IF NOT (num >= 1) GOTO _CV
    VARIABLE num = num - 1
    VARIABLE RET_CW
    _CW:
    IF NOT (num >= 1) GOTO _CY
    VARIABLE num = num - 1
    VARIABLE RET_CZ
    _CZ:
    IF NOT (num >= 1) GOTO _DB
    VARIABLE num = num - 1
    VARIABLE RET_DC
    _DC:
    IF NOT (num >= 1) GOTO _DE
    VARIABLE num = num - 1
    VARIABLE RET_DF
    _DF:
    IF NOT (num >= 1) GOTO _DH
    VARIABLE num = num - 1
    VARIABLE RET_DI
    _DI:
    IF NOT (num >= 1) GOTO _DK
    VARIABLE num = num - 1
    VARIABLE RET_DL
    _DL:
    IF NOT (num >= 1) GOTO _DN
    VARIABLE num = num - 1
    VARIABLE RET_DO
    _DO:
    IF NOT (num >= 1) GOTO _DQ
    VARIABLE num = num - 1
    VARIABLE RET_DR
    _DR:
    IF NOT (num >= 1) GOTO _DT
    VARIABLE num = num - 1
    VARIABLE RET_DU
    _DU:
    IF NOT (num >= 1) GOTO _DW
    VARIABLE num = num - 1
    VARIABLE RET_DX
    _DX:
    IF NOT (num >= 1) GOTO _DZ
    VARIABLE num = num - 1
    VARIABLE RET_EA
    _EA:
    IF NOT (num >= 1) GOTO _EC
    VARIABLE num = num - 1
    VARIABLE RET_ED
    _ED:
    IF NOT (num >= 1) GOTO _EF
    VARIABLE num = num - 1
    VARIABLE RET_EG
    _EG:
    IF NOT (num >= 1) GOTO _EI
    VARIABLE num = num - 1
    VARIABLE RET_EJ
    _EJ:
    IF NOT (num >= 1) GOTO _EL
    VARIABLE num = num - 1
    VARIABLE RET_EM
    _EM:
    IF NOT (num >= 1) GOTO _EO
    VARIABLE num = num - 1
    VARIABLE RET_EP
    _EP:
    IF NOT (num >= 1) GOTO _ER
    VARIABLE num = num - 1
    VARIABLE RET_ES
    _ES:
    IF NOT (num >= 1) GOTO _EU
    VARIABLE num = num - 1
    VARIABLE RET_EV
    _EV:
    IF NOT (num >= 1) GOTO _EX
    VARIABLE num = num - 1
    VARIABLE RET_EY
    _EY:
    IF NOT (num >= 1) GOTO _FA
    VARIABLE num = num - 1
    VARIABLE RET_FB
    _FB:
    IF NOT (num >= 1) GOTO _FD
    VARIABLE num = num - 1
    VARIABLE RET_FE
    _FE:
    IF NOT (num >= 1) GOTO _FG
    VARIABLE num = num - 1
    VARIABLE RET_FH
    _FH:
    IF NOT (num >= 1) GOTO _FJ
    VARIABLE num = num - 1
    VARIABLE RET_FK
    _FK:
    IF NOT (num >= 1) GOTO _FM
    VARIABLE num = num - 1
    VARIABLE RET_FN
    _FN:
    IF NOT (num >= 1) GOTO _FP
    VARIABLE num = num - 1
    VARIABLE RET_FQ
    _FQ:
    IF NOT (num >= 1) GOTO _FS
    VARIABLE num = num - 1
    VARIABLE RET_FT
    _FT:
    IF NOT (num >= 1) GOTO _FV
    VARIABLE num = num - 1
    VARIABLE RET_FW
    _FW:
    IF NOT (num >= 1) GOTO _FY
    VARIABLE num = num - 1
    VARIABLE RET_FZ
    _FZ:
    IF NOT (num >= 1) GOTO _GB
    VARIABLE num = num - 1
    VARIABLE RET_GC
    _GC:
    IF NOT (num >= 1) GOTO _GE
    VARIABLE num = num - 1
    VARIABLE RET_GF
    _GF:
    IF NOT (num >= 1) GOTO _GH
    VARIABLE num = num - 1
    VARIABLE RET_GI
    _GI:
    IF NOT (num >= 1) GOTO _GK
    VARIABLE num = num - 1
    VARIABLE RET_GL
    _GL:
    IF NOT (num >= 1) GOTO _GN
    VARIABLE num = num - 1
    VARIABLE RET_GO
    _GO:
    IF NOT (num >= 1) GOTO _GQ
    VARIABLE num = num - 1
    VARIABLE RET_GR
    _GR:
    IF NOT (num >= 1) GOTO _GT
    VARIABLE num = num - 1
    VARIABLE RET_GU
    _GU:
    IF NOT (num >= 1) GOTO _GW
    VARIABLE num = num - 1
    VARIABLE RET_GX
    _GX:
    IF NOT (num >= 1) GOTO _GZ
    VARIABLE num = num - 1
    VARIABLE RET_HA
    _HA:
    IF NOT (num >= 1) GOTO _HC
    VARIABLE num = num - 1
    VARIABLE RET_HD
    _HD:
    IF NOT (num >= 1) GOTO _HF
    VARIABLE num = num - 1
    VARIABLE RET_HG
    _HG:
    IF NOT (num >= 1) GOTO _HI
    VARIABLE num = num - 1
    VARIABLE RET_HJ
    _HJ:
    IF NOT (num >= 1) GOTO _HL
    VARIABLE num = num - 1
    VARIABLE RET_HM
    _HM:
    IF NOT (num >= 1) GOTO _HO
    VARIABLE num = num - 1
    VARIABLE RET_HP
    _HP:
    IF NOT (num >= 1) GOTO _HR
    VARIABLE num = num - 1
    VARIABLE RET_HS
    _HS:
    IF NOT (num >= 1) GOTO _HU
    VARIABLE num = num - 1
    VARIABLE RET_HV
    _HV:
    IF NOT (num >= 1) GOTO _HX
    VARIABLE num = num - 1
    VARIABLE RET_HY
    _HY:
    IF NOT (num >= 1) GOTO _IA
    VARIABLE num = num - 1
    VARIABLE RET_IB
    _IB:
    IF NOT (num >= 1) GOTO _ID
    VARIABLE num = num - 1
    VARIABLE RET_IE
    _IE:
    IF NOT (num >= 1) GOTO _IG
    VARIABLE num = num - 1
    VARIABLE RET_IH
    _IH:
    IF NOT (num >= 1) GOTO _IJ
    VARIABLE num = num - 1
    VARIABLE RET_IK
    _IK:
    IF NOT (num >= 1) GOTO _IM
    VARIABLE num = num - 1
    VARIABLE RET_IN
    _IN:
    IF NOT (num >= 1) GOTO _IP
    VARIABLE num = num - 1
    VARIABLE RET_IQ
    _IQ:
    IF NOT (num >= 1) GOTO _IS
    VARIABLE num = num - 1
    VARIABLE RET_IT
    _IT:
    IF NOT (num >= 1) GOTO _IV
    VARIABLE num = num - 1
    VARIABLE RET_IW
    _IW:
    IF NOT (num >= 1) GOTO _IY
    VARIABLE num = num - 1
    VARIABLE RET_IZ
    _IZ:
    IF NOT (num >= 1) GOTO _JB
    VARIABLE num = num - 1
    VARIABLE RET_JC
    _JC:
    IF NOT (num >= 1) GOTO _JE
    VARIABLE num = num - 1
    VARIABLE RET_JF
    _JF:
    IF NOT (num >= 1) GOTO _JH
    VARIABLE num = num - 1
    VARIABLE RET_JI
    _JI:
    IF NOT (num >= 1) GOTO _JK
    VARIABLE num = num - 1
    VARIABLE RET_JL
    _JL:
    IF NOT (num >= 1) GOTO _JN
    VARIABLE num = num - 1
    VARIABLE RET_JO
    _JO:
    IF NOT (num >= 1) GOTO _JQ
    VARIABLE num = num - 1
    VARIABLE RET_JR
    _JR:
    IF NOT (num >= 1) GOTO _JT
    VARIABLE num = num - 1
    VARIABLE RET_JU
    _JU:
    IF NOT (num >= 1) GOTO _JW
    VARIABLE num = num - 1
    VARIABLE RET_JX
    _JX:
    IF NOT (num >= 1) GOTO _JZ
    VARIABLE num = num - 1
    VARIABLE RET_KA
    _KA:
    IF NOT (num >= 1) GOTO _KC
    VARIABLE num = num - 1
    VARIABLE RET_KD
    _KD:
    IF NOT (num >= 1) GOTO _KF
    VARIABLE num = num - 1
    VARIABLE RET_KG
    _KG:
    IF NOT (num >= 1) GOTO _KI
    VARIABLE num = num - 1
    VARIABLE RET_KJ
    _KJ:
    IF NOT (num >= 1) GOTO _KL
    VARIABLE num = num - 1
    VARIABLE RET_KM
    _KM:
    IF NOT (num >= 1) GOTO _KO
    RET_KM = Method1(num - 1)
    GOTO _KN
    _KO:
    RET_KM = num
    GOTO _KN
    _KN:
    RET_KJ = RET_KM
    GOTO _KK
    _KL:
    RET_KJ = num
    GOTO _KK
    _KK:
    RET_KG = RET_KJ
    GOTO _KH
    _KI:
    RET_KG = num
    GOTO _KH
    _KH:
    RET_KD = RET_KG
    GOTO _KE
    _KF:
    RET_KD = num
    GOTO _KE
    _KE:
    RET_KA = RET_KD
    GOTO _KB
    _KC:
    RET_KA = num
    GOTO _KB
    _KB:
    RET_JX = RET_KA
    GOTO _JY
    _JZ:
    RET_JX = num
    GOTO _JY
    _JY:
    RET_JU = RET_JX
    GOTO _JV
    _JW:
    RET_JU = num
    GOTO _JV
    _JV:
    RET_JR = RET_JU
    GOTO _JS
    _JT:
    RET_JR = num
    GOTO _JS
    _JS:
    RET_JO = RET_JR
    GOTO _JP
    _JQ:
    RET_JO = num
    GOTO _JP
    _JP:
    RET_JL = RET_JO
    GOTO _JM
    _JN:
    RET_JL = num
    GOTO _JM
    _JM:
    RET_JI = RET_JL
    GOTO _JJ
    _JK:
    RET_JI = num
    GOTO _JJ
    _JJ:
    RET_JF = RET_JI
    GOTO _JG
    _JH:
    RET_JF = num
    GOTO _JG
    _JG:
    RET_JC = RET_JF
    GOTO _JD
    _JE:
    RET_JC = num
    GOTO _JD
    _JD:
    RET_IZ = RET_JC
    GOTO _JA
    _JB:
    RET_IZ = num
    GOTO _JA
    _JA:
    RET_IW = RET_IZ
    GOTO _IX
    _IY:
    RET_IW = num
    GOTO _IX
    _IX:
    RET_IT = RET_IW
    GOTO _IU
    _IV:
    RET_IT = num
    GOTO _IU
    _IU:
    RET_IQ = RET_IT
    GOTO _IR
    _IS:
    RET_IQ = num
    GOTO _IR
    _IR:
    RET_IN = RET_IQ
    GOTO _IO
    _IP:
    RET_IN = num
    GOTO _IO
    _IO:
    RET_IK = RET_IN
    GOTO _IL
    _IM:
    RET_IK = num
    GOTO _IL
    _IL:
    RET_IH = RET_IK
    GOTO _II
    _IJ:
    RET_IH = num
    GOTO _II
    _II:
    RET_IE = RET_IH
    GOTO _IF
    _IG:
    RET_IE = num
    GOTO _IF
    _IF:
    RET_IB = RET_IE
    GOTO _IC
    _ID:
    RET_IB = num
    GOTO _IC
    _IC:
    RET_HY = RET_IB
    GOTO _HZ
    _IA:
    RET_HY = num
    GOTO _HZ
    _HZ:
    RET_HV = RET_HY
    GOTO _HW
    _HX:
    RET_HV = num
    GOTO _HW
    _HW:
    RET_HS = RET_HV
    GOTO _HT
    _HU:
    RET_HS = num
    GOTO _HT
    _HT:
    RET_HP = RET_HS
    GOTO _HQ
    _HR:
    RET_HP = num
    GOTO _HQ
    _HQ:
    RET_HM = RET_HP
    GOTO _HN
    _HO:
    RET_HM = num
    GOTO _HN
    _HN:
    RET_HJ = RET_HM
    GOTO _HK
    _HL:
    RET_HJ = num
    GOTO _HK
    _HK:
    RET_HG = RET_HJ
    GOTO _HH
    _HI:
    RET_HG = num
    GOTO _HH
    _HH:
    RET_HD = RET_HG
    GOTO _HE
    _HF:
    RET_HD = num
    GOTO _HE
    _HE:
    RET_HA = RET_HD
    GOTO _HB
    _HC:
    RET_HA = num
    GOTO _HB
    _HB:
    RET_GX = RET_HA
    GOTO _GY
    _GZ:
    RET_GX = num
    GOTO _GY
    _GY:
    RET_GU = RET_GX
    GOTO _GV
    _GW:
    RET_GU = num
    GOTO _GV
    _GV:
    RET_GR = RET_GU
    GOTO _GS
    _GT:
    RET_GR = num
    GOTO _GS
    _GS:
    RET_GO = RET_GR
    GOTO _GP
    _GQ:
    RET_GO = num
    GOTO _GP
    _GP:
    RET_GL = RET_GO
    GOTO _GM
    _GN:
    RET_GL = num
    GOTO _GM
    _GM:
    RET_GI = RET_GL
    GOTO _GJ
    _GK:
    RET_GI = num
    GOTO _GJ
    _GJ:
    RET_GF = RET_GI
    GOTO _GG
    _GH:
    RET_GF = num
    GOTO _GG
    _GG:
    RET_GC = RET_GF
    GOTO _GD
    _GE:
    RET_GC = num
    GOTO _GD
    _GD:
    RET_FZ = RET_GC
    GOTO _GA
    _GB:
    RET_FZ = num
    GOTO _GA
    _GA:
    RET_FW = RET_FZ
    GOTO _FX
    _FY:
    RET_FW = num
    GOTO _FX
    _FX:
    RET_FT = RET_FW
    GOTO _FU
    _FV:
    RET_FT = num
    GOTO _FU
    _FU:
    RET_FQ = RET_FT
    GOTO _FR
    _FS:
    RET_FQ = num
    GOTO _FR
    _FR:
    RET_FN = RET_FQ
    GOTO _FO
    _FP:
    RET_FN = num
    GOTO _FO
    _FO:
    RET_FK = RET_FN
    GOTO _FL
    _FM:
    RET_FK = num
    GOTO _FL
    _FL:
    RET_FH = RET_FK
    GOTO _FI
    _FJ:
    RET_FH = num
    GOTO _FI
    _FI:
    RET_FE = RET_FH
    GOTO _FF
    _FG:
    RET_FE = num
    GOTO _FF
    _FF:
    RET_FB = RET_FE
    GOTO _FC
    _FD:
    RET_FB = num
    GOTO _FC
    _FC:
    RET_EY = RET_FB
    GOTO _EZ
    _FA:
    RET_EY = num
    GOTO _EZ
    _EZ:
    RET_EV = RET_EY
    GOTO _EW
    _EX:
    RET_EV = num
    GOTO _EW
    _EW:
    RET_ES = RET_EV
    GOTO _ET
    _EU:
    RET_ES = num
    GOTO _ET
    _ET:
    RET_EP = RET_ES
    GOTO _EQ
    _ER:
    RET_EP = num
    GOTO _EQ
    _EQ:
    RET_EM = RET_EP
    GOTO _EN
    _EO:
    RET_EM = num
    GOTO _EN
    _EN:
    RET_EJ = RET_EM
    GOTO _EK
    _EL:
    RET_EJ = num
    GOTO _EK
    _EK:
    RET_EG = RET_EJ
    GOTO _EH
    _EI:
    RET_EG = num
    GOTO _EH
    _EH:
    RET_ED = RET_EG
    GOTO _EE
    _EF:
    RET_ED = num
    GOTO _EE
    _EE:
    RET_EA = RET_ED
    GOTO _EB
    _EC:
    RET_EA = num
    GOTO _EB
    _EB:
    RET_DX = RET_EA
    GOTO _DY
    _DZ:
    RET_DX = num
    GOTO _DY
    _DY:
    RET_DU = RET_DX
    GOTO _DV
    _DW:
    RET_DU = num
    GOTO _DV
    _DV:
    RET_DR = RET_DU
    GOTO _DS
    _DT:
    RET_DR = num
    GOTO _DS
    _DS:
    RET_DO = RET_DR
    GOTO _DP
    _DQ:
    RET_DO = num
    GOTO _DP
    _DP:
    RET_DL = RET_DO
    GOTO _DM
    _DN:
    RET_DL = num
    GOTO _DM
    _DM:
    RET_DI = RET_DL
    GOTO _DJ
    _DK:
    RET_DI = num
    GOTO _DJ
    _DJ:
    RET_DF = RET_DI
    GOTO _DG
    _DH:
    RET_DF = num
    GOTO _DG
    _DG:
    RET_DC = RET_DF
    GOTO _DD
    _DE:
    RET_DC = num
    GOTO _DD
    _DD:
    RET_CZ = RET_DC
    GOTO _DA
    _DB:
    RET_CZ = num
    GOTO _DA
    _DA:
    RET_CW = RET_CZ
    GOTO _CX
    _CY:
    RET_CW = num
    GOTO _CX
    _CX:
    RET_CT = RET_CW
    GOTO _CU
    _CV:
    RET_CT = num
    GOTO _CU
    _CU:
    RET_CQ = RET_CT
    GOTO _CR
    _CS:
    RET_CQ = num
    GOTO _CR
    _CR:
    RET_CN = RET_CQ
    GOTO _CO
    _CP:
    RET_CN = num
    GOTO _CO
    _CO:
    RET_CK = RET_CN
    GOTO _CL
    _CM:
    RET_CK = num
    GOTO _CL
    _CL:
    RET_CH = RET_CK
    GOTO _CI
    _CJ:
    RET_CH = num
    GOTO _CI
    _CI:
    RET_CE = RET_CH
    GOTO _CF
    _CG:
    RET_CE = num
    GOTO _CF
    _CF:
    RET_CB = RET_CE
    GOTO _CC
    _CD:
    RET_CB = num
    GOTO _CC
    _CC:
    RET_BY = RET_CB
    GOTO _BZ
    _CA:
    RET_BY = num
    GOTO _BZ
    _BZ:
    RET_BV = RET_BY
    GOTO _BW
    _BX:
    RET_BV = num
    GOTO _BW
    _BW:
    RET_BS = RET_BV
    GOTO _BT
    _BU:
    RET_BS = num
    GOTO _BT
    _BT:
    RET_BP = RET_BS
    GOTO _BQ
    _BR:
    RET_BP = num
    GOTO _BQ
    _BQ:
    RET_BM = RET_BP
    GOTO _BN
    _BO:
    RET_BM = num
    GOTO _BN
    _BN:
    RET_BJ = RET_BM
    GOTO _BK
    _BL:
    RET_BJ = num
    GOTO _BK
    _BK:
    RET_BG = RET_BJ
    GOTO _BH
    _BI:
    RET_BG = num
    GOTO _BH
    _BH:
    RET_BD = RET_BG
    GOTO _BE
    _BF:
    RET_BD = num
    GOTO _BE
    _BE:
    RET_BA = RET_BD
    GOTO _BB
    _BC:
    RET_BA = num
    GOTO _BB
    _BB:
    RET_AX = RET_BA
    GOTO _AY
    _AZ:
    RET_AX = num
    GOTO _AY
    _AY:
    RET_AU = RET_AX
    GOTO _AV
    _AW:
    RET_AU = num
    GOTO _AV
    _AV:
    RET_AR = RET_AU
    GOTO _AS
    _AT:
    RET_AR = num
    GOTO _AS
    _AS:
    RET_AO = RET_AR
    GOTO _AP
    _AQ:
    RET_AO = num
    GOTO _AP
    _AP:
    RET_AL = RET_AO
    GOTO _AM
    _AN:
    RET_AL = num
    GOTO _AM
    _AM:
    RET_AI = RET_AL
    GOTO _AJ
    _AK:
    RET_AI = num
    GOTO _AJ
    _AJ:
    RET_AF = RET_AI
    GOTO _AG
    _AH:
    RET_AF = num
    GOTO _AG
    _AG:
    RET_AC = RET_AF
    GOTO _AD
    _AE:
    RET_AC = num
    GOTO _AD
    _AD:
    RET_Z = RET_AC
    GOTO _AA
    _AB:
    RET_Z = num
    GOTO _AA
    _AA:
    RET_W = RET_Z
    GOTO _X
    _Y:
    RET_W = num
    GOTO _X
    _X:
    RET_T = RET_W
    GOTO _U
    _V:
    RET_T = num
    GOTO _U
    _U:
    RET_Q = RET_T
    GOTO _R
    _S:
    RET_Q = num
    GOTO _R
    _R:
    RET_N = RET_Q
    GOTO _O
    _P:
    RET_N = num
    GOTO _O
    _O:
    RET_K = RET_N
    GOTO _L
    _M:
    RET_K = num
    GOTO _L
    _L:
    RET_H = RET_K
    GOTO _I
    _J:
    RET_H = num
    GOTO _I
    _I:
    RET_E = RET_H
    GOTO _F
    _G:
    RET_E = num
    GOTO _F
    _F:
    RET_B = RET_E
    GOTO _C
    _D:
    RET_B = num
    GOTO _C
    _C:
    RET_A = RET_B
    RETURN RET_A
END FUNCTION

FUNCTION Method1(num)
    IF NOT (num >= 1) GOTO _KP
    VARIABLE RET_KQ
    VARIABLE num = num - 1
    VARIABLE RET_KR
    _KR:
    IF NOT (num >= 1) GOTO _KT
    VARIABLE num = num - 1
    VARIABLE RET_KU
    _KU:
    IF NOT (num >= 1) GOTO _KW
    VARIABLE num = num - 1
    VARIABLE RET_KX
    _KX:
    IF NOT (num >= 1) GOTO _KZ
    VARIABLE num = num - 1
    VARIABLE RET_LA
    _LA:
    IF NOT (num >= 1) GOTO _LC
    VARIABLE num = num - 1
    VARIABLE RET_LD
    _LD:
    IF NOT (num >= 1) GOTO _LF
    VARIABLE num = num - 1
    VARIABLE RET_LG
    _LG:
    IF NOT (num >= 1) GOTO _LI
    VARIABLE num = num - 1
    VARIABLE RET_LJ
    _LJ:
    IF NOT (num >= 1) GOTO _LL
    VARIABLE num = num - 1
    VARIABLE RET_LM
    _LM:
    IF NOT (num >= 1) GOTO _LO
    VARIABLE num = num - 1
    VARIABLE RET_LP
    _LP:
    IF NOT (num >= 1) GOTO _LR
    VARIABLE num = num - 1
    VARIABLE RET_LS
    _LS:
    IF NOT (num >= 1) GOTO _LU
    VARIABLE num = num - 1
    VARIABLE RET_LV
    _LV:
    IF NOT (num >= 1) GOTO _LX
    VARIABLE num = num - 1
    VARIABLE RET_LY
    _LY:
    IF NOT (num >= 1) GOTO _MA
    VARIABLE num = num - 1
    VARIABLE RET_MB
    _MB:
    IF NOT (num >= 1) GOTO _MD
    VARIABLE num = num - 1
    VARIABLE RET_ME
    _ME:
    IF NOT (num >= 1) GOTO _MG
    VARIABLE num = num - 1
    VARIABLE RET_MH
    _MH:
    IF NOT (num >= 1) GOTO _MJ
    VARIABLE num = num - 1
    VARIABLE RET_MK
    _MK:
    IF NOT (num >= 1) GOTO _MM
    VARIABLE num = num - 1
    VARIABLE RET_MN
    _MN:
    IF NOT (num >= 1) GOTO _MP
    VARIABLE num = num - 1
    VARIABLE RET_MQ
    _MQ:
    IF NOT (num >= 1) GOTO _MS
    VARIABLE num = num - 1
    VARIABLE RET_MT
    _MT:
    IF NOT (num >= 1) GOTO _MV
    VARIABLE num = num - 1
    VARIABLE RET_MW
    _MW:
    IF NOT (num >= 1) GOTO _MY
    VARIABLE num = num - 1
    VARIABLE RET_MZ
    _MZ:
    IF NOT (num >= 1) GOTO _NB
    VARIABLE num = num - 1
    VARIABLE RET_NC
    _NC:
    IF NOT (num >= 1) GOTO _NE
    VARIABLE num = num - 1
    VARIABLE RET_NF
    _NF:
    IF NOT (num >= 1) GOTO _NH
    VARIABLE num = num - 1
    VARIABLE RET_NI
    _NI:
    IF NOT (num >= 1) GOTO _NK
    VARIABLE num = num - 1
    VARIABLE RET_NL
    _NL:
    IF NOT (num >= 1) GOTO _NN
    VARIABLE num = num - 1
    VARIABLE RET_NO
    _NO:
    IF NOT (num >= 1) GOTO _NQ
    VARIABLE num = num - 1
    VARIABLE RET_NR
    _NR:
    IF NOT (num >= 1) GOTO _NT
    VARIABLE num = num - 1
    VARIABLE RET_NU
    _NU:
    IF NOT (num >= 1) GOTO _NW
    VARIABLE num = num - 1
    VARIABLE RET_NX
    _NX:
    IF NOT (num >= 1) GOTO _NZ
    VARIABLE num = num - 1
    VARIABLE RET_OA
    _OA:
    IF NOT (num >= 1) GOTO _OC
    VARIABLE num = num - 1
    VARIABLE RET_OD
    _OD:
    IF NOT (num >= 1) GOTO _OF
    VARIABLE num = num - 1
    VARIABLE RET_OG
    _OG:
    IF NOT (num >= 1) GOTO _OI
    VARIABLE num = num - 1
    VARIABLE RET_OJ
    _OJ:
    IF NOT (num >= 1) GOTO _OL
    VARIABLE num = num - 1
    VARIABLE RET_OM
    _OM:
    IF NOT (num >= 1) GOTO _OO
    VARIABLE num = num - 1
    VARIABLE RET_OP
    _OP:
    IF NOT (num >= 1) GOTO _OR
    VARIABLE num = num - 1
    VARIABLE RET_OS
    _OS:
    IF NOT (num >= 1) GOTO _OU
    VARIABLE num = num - 1
    VARIABLE RET_OV
    _OV:
    IF NOT (num >= 1) GOTO _OX
    VARIABLE num = num - 1
    VARIABLE RET_OY
    _OY:
    IF NOT (num >= 1) GOTO _PA
    VARIABLE num = num - 1
    VARIABLE RET_PB
    _PB:
    IF NOT (num >= 1) GOTO _PD
    VARIABLE num = num - 1
    VARIABLE RET_PE
    _PE:
    IF NOT (num >= 1) GOTO _PG
    VARIABLE num = num - 1
    VARIABLE RET_PH
    _PH:
    IF NOT (num >= 1) GOTO _PJ
    VARIABLE num = num - 1
    VARIABLE RET_PK
    _PK:
    IF NOT (num >= 1) GOTO _PM
    VARIABLE num = num - 1
    VARIABLE RET_PN
    _PN:
    IF NOT (num >= 1) GOTO _PP
    VARIABLE num = num - 1
    VARIABLE RET_PQ
    _PQ:
    IF NOT (num >= 1) GOTO _PS
    VARIABLE num = num - 1
    VARIABLE RET_PT
    _PT:
    IF NOT (num >= 1) GOTO _PV
    VARIABLE num = num - 1
    VARIABLE RET_PW
    _PW:
    IF NOT (num >= 1) GOTO _PY
    VARIABLE num = num - 1
    VARIABLE RET_PZ
    _PZ:
    IF NOT (num >= 1) GOTO _QB
    VARIABLE num = num - 1
    VARIABLE RET_QC
    _QC:
    IF NOT (num >= 1) GOTO _QE
    VARIABLE num = num - 1
    VARIABLE RET_QF
    _QF:
    IF NOT (num >= 1) GOTO _QH
    VARIABLE num = num - 1
    VARIABLE RET_QI
    _QI:
    IF NOT (num >= 1) GOTO _QK
    VARIABLE num = num - 1
    VARIABLE RET_QL
    _QL:
    IF NOT (num >= 1) GOTO _QN
    VARIABLE num = num - 1
    VARIABLE RET_QO
    _QO:
    IF NOT (num >= 1) GOTO _QQ
    VARIABLE num = num - 1
    VARIABLE RET_QR
    _QR:
    IF NOT (num >= 1) GOTO _QT
    VARIABLE num = num - 1
    VARIABLE RET_QU
    _QU:
    IF NOT (num >= 1) GOTO _QW
    VARIABLE num = num - 1
    VARIABLE RET_QX
    _QX:
    IF NOT (num >= 1) GOTO _QZ
    VARIABLE num = num - 1
    VARIABLE RET_RA
    _RA:
    IF NOT (num >= 1) GOTO _RC
    VARIABLE num = num - 1
    VARIABLE RET_RD
    _RD:
    IF NOT (num >= 1) GOTO _RF
    VARIABLE num = num - 1
    VARIABLE RET_RG
    _RG:
    IF NOT (num >= 1) GOTO _RI
    VARIABLE num = num - 1
    VARIABLE RET_RJ
    _RJ:
    IF NOT (num >= 1) GOTO _RL
    VARIABLE num = num - 1
    VARIABLE RET_RM
    _RM:
    IF NOT (num >= 1) GOTO _RO
    VARIABLE num = num - 1
    VARIABLE RET_RP
    _RP:
    IF NOT (num >= 1) GOTO _RR
    VARIABLE num = num - 1
    VARIABLE RET_RS
    _RS:
    IF NOT (num >= 1) GOTO _RU
    VARIABLE num = num - 1
    VARIABLE RET_RV
    _RV:
    IF NOT (num >= 1) GOTO _RX
    VARIABLE num = num - 1
    VARIABLE RET_RY
    _RY:
    IF NOT (num >= 1) GOTO _SA
    VARIABLE num = num - 1
    VARIABLE RET_SB
    _SB:
    IF NOT (num >= 1) GOTO _SD
    VARIABLE num = num - 1
    VARIABLE RET_SE
    _SE:
    IF NOT (num >= 1) GOTO _SG
    VARIABLE num = num - 1
    VARIABLE RET_SH
    _SH:
    IF NOT (num >= 1) GOTO _SJ
    VARIABLE num = num - 1
    VARIABLE RET_SK
    _SK:
    IF NOT (num >= 1) GOTO _SM
    VARIABLE num = num - 1
    VARIABLE RET_SN
    _SN:
    IF NOT (num >= 1) GOTO _SP
    VARIABLE num = num - 1
    VARIABLE RET_SQ
    _SQ:
    IF NOT (num >= 1) GOTO _SS
    VARIABLE num = num - 1
    VARIABLE RET_ST
    _ST:
    IF NOT (num >= 1) GOTO _SV
    VARIABLE num = num - 1
    VARIABLE RET_SW
    _SW:
    IF NOT (num >= 1) GOTO _SY
    VARIABLE num = num - 1
    VARIABLE RET_SZ
    _SZ:
    IF NOT (num >= 1) GOTO _TB
    VARIABLE num = num - 1
    VARIABLE RET_TC
    _TC:
    IF NOT (num >= 1) GOTO _TE
    VARIABLE num = num - 1
    VARIABLE RET_TF
    _TF:
    IF NOT (num >= 1) GOTO _TH
    VARIABLE num = num - 1
    VARIABLE RET_TI
    _TI:
    IF NOT (num >= 1) GOTO _TK
    VARIABLE num = num - 1
    VARIABLE RET_TL
    _TL:
    IF NOT (num >= 1) GOTO _TN
    VARIABLE num = num - 1
    VARIABLE RET_TO
    _TO:
    IF NOT (num >= 1) GOTO _TQ
    VARIABLE num = num - 1
    VARIABLE RET_TR
    _TR:
    IF NOT (num >= 1) GOTO _TT
    VARIABLE num = num - 1
    VARIABLE RET_TU
    _TU:
    IF NOT (num >= 1) GOTO _TW
    VARIABLE num = num - 1
    VARIABLE RET_TX
    _TX:
    IF NOT (num >= 1) GOTO _TZ
    VARIABLE num = num - 1
    VARIABLE RET_UA
    _UA:
    IF NOT (num >= 1) GOTO _UC
    VARIABLE num = num - 1
    VARIABLE RET_UD
    _UD:
    IF NOT (num >= 1) GOTO _UF
    VARIABLE num = num - 1
    VARIABLE RET_UG
    _UG:
    IF NOT (num >= 1) GOTO _UI
    VARIABLE num = num - 1
    VARIABLE RET_UJ
    _UJ:
    IF NOT (num >= 1) GOTO _UL
    VARIABLE num = num - 1
    VARIABLE RET_UM
    _UM:
    IF NOT (num >= 1) GOTO _UO
    VARIABLE num = num - 1
    VARIABLE RET_UP
    _UP:
    IF NOT (num >= 1) GOTO _UR
    VARIABLE num = num - 1
    VARIABLE RET_US
    _US:
    IF NOT (num >= 1) GOTO _UU
    VARIABLE num = num - 1
    VARIABLE RET_UV
    _UV:
    IF NOT (num >= 1) GOTO _UX
    VARIABLE num = num - 1
    VARIABLE RET_UY
    _UY:
    IF NOT (num >= 1) GOTO _VA
    VARIABLE num = num - 1
    VARIABLE RET_VB
    _VB:
    IF NOT (num >= 1) GOTO _VD
    VARIABLE num = num - 1
    VARIABLE RET_VE
    _VE:
    IF NOT (num >= 1) GOTO _VG
    VARIABLE num = num - 1
    VARIABLE RET_VH
    _VH:
    IF NOT (num >= 1) GOTO _VJ
    VARIABLE num = num - 1
    VARIABLE RET_VK
    _VK:
    IF NOT (num >= 1) GOTO _VM
    VARIABLE num = num - 1
    VARIABLE RET_VN
    _VN:
    IF NOT (num >= 1) GOTO _VP
    VARIABLE num = num - 1
    VARIABLE RET_VQ
    _VQ:
    IF NOT (num >= 1) GOTO _VS
    VARIABLE num = num - 1
    VARIABLE RET_VT
    _VT:
    IF NOT (num >= 1) GOTO _VV
    VARIABLE num = num - 1
    VARIABLE RET_VW
    _VW:
    IF NOT (num >= 1) GOTO _VY
    VARIABLE num = num - 1
    VARIABLE RET_VZ
    _VZ:
    IF NOT (num >= 1) GOTO _WB
    VARIABLE num = num - 1
    VARIABLE RET_WC
    _WC:
    IF NOT (num >= 1) GOTO _WE
    RET_WC = Method1(num - 1)
    GOTO _WD
    _WE:
    RET_WC = num
    GOTO _WD
    _WD:
    RET_VZ = RET_WC
    GOTO _WA
    _WB:
    RET_VZ = num
    GOTO _WA
    _WA:
    RET_VW = RET_VZ
    GOTO _VX
    _VY:
    RET_VW = num
    GOTO _VX
    _VX:
    RET_VT = RET_VW
    GOTO _VU
    _VV:
    RET_VT = num
    GOTO _VU
    _VU:
    RET_VQ = RET_VT
    GOTO _VR
    _VS:
    RET_VQ = num
    GOTO _VR
    _VR:
    RET_VN = RET_VQ
    GOTO _VO
    _VP:
    RET_VN = num
    GOTO _VO
    _VO:
    RET_VK = RET_VN
    GOTO _VL
    _VM:
    RET_VK = num
    GOTO _VL
    _VL:
    RET_VH = RET_VK
    GOTO _VI
    _VJ:
    RET_VH = num
    GOTO _VI
    _VI:
    RET_VE = RET_VH
    GOTO _VF
    _VG:
    RET_VE = num
    GOTO _VF
    _VF:
    RET_VB = RET_VE
    GOTO _VC
    _VD:
    RET_VB = num
    GOTO _VC
    _VC:
    RET_UY = RET_VB
    GOTO _UZ
    _VA:
    RET_UY = num
    GOTO _UZ
    _UZ:
    RET_UV = RET_UY
    GOTO _UW
    _UX:
    RET_UV = num
    GOTO _UW
    _UW:
    RET_US = RET_UV
    GOTO _UT
    _UU:
    RET_US = num
    GOTO _UT
    _UT:
    RET_UP = RET_US
    GOTO _UQ
    _UR:
    RET_UP = num
    GOTO _UQ
    _UQ:
    RET_UM = RET_UP
    GOTO _UN
    _UO:
    RET_UM = num
    GOTO _UN
    _UN:
    RET_UJ = RET_UM
    GOTO _UK
    _UL:
    RET_UJ = num
    GOTO _UK
    _UK:
    RET_UG = RET_UJ
    GOTO _UH
    _UI:
    RET_UG = num
    GOTO _UH
    _UH:
    RET_UD = RET_UG
    GOTO _UE
    _UF:
    RET_UD = num
    GOTO _UE
    _UE:
    RET_UA = RET_UD
    GOTO _UB
    _UC:
    RET_UA = num
    GOTO _UB
    _UB:
    RET_TX = RET_UA
    GOTO _TY
    _TZ:
    RET_TX = num
    GOTO _TY
    _TY:
    RET_TU = RET_TX
    GOTO _TV
    _TW:
    RET_TU = num
    GOTO _TV
    _TV:
    RET_TR = RET_TU
    GOTO _TS
    _TT:
    RET_TR = num
    GOTO _TS
    _TS:
    RET_TO = RET_TR
    GOTO _TP
    _TQ:
    RET_TO = num
    GOTO _TP
    _TP:
    RET_TL = RET_TO
    GOTO _TM
    _TN:
    RET_TL = num
    GOTO _TM
    _TM:
    RET_TI = RET_TL
    GOTO _TJ
    _TK:
    RET_TI = num
    GOTO _TJ
    _TJ:
    RET_TF = RET_TI
    GOTO _TG
    _TH:
    RET_TF = num
    GOTO _TG
    _TG:
    RET_TC = RET_TF
    GOTO _TD
    _TE:
    RET_TC = num
    GOTO _TD
    _TD:
    RET_SZ = RET_TC
    GOTO _TA
    _TB:
    RET_SZ = num
    GOTO _TA
    _TA:
    RET_SW = RET_SZ
    GOTO _SX
    _SY:
    RET_SW = num
    GOTO _SX
    _SX:
    RET_ST = RET_SW
    GOTO _SU
    _SV:
    RET_ST = num
    GOTO _SU
    _SU:
    RET_SQ = RET_ST
    GOTO _SR
    _SS:
    RET_SQ = num
    GOTO _SR
    _SR:
    RET_SN = RET_SQ
    GOTO _SO
    _SP:
    RET_SN = num
    GOTO _SO
    _SO:
    RET_SK = RET_SN
    GOTO _SL
    _SM:
    RET_SK = num
    GOTO _SL
    _SL:
    RET_SH = RET_SK
    GOTO _SI
    _SJ:
    RET_SH = num
    GOTO _SI
    _SI:
    RET_SE = RET_SH
    GOTO _SF
    _SG:
    RET_SE = num
    GOTO _SF
    _SF:
    RET_SB = RET_SE
    GOTO _SC
    _SD:
    RET_SB = num
    GOTO _SC
    _SC:
    RET_RY = RET_SB
    GOTO _RZ
    _SA:
    RET_RY = num
    GOTO _RZ
    _RZ:
    RET_RV = RET_RY
    GOTO _RW
    _RX:
    RET_RV = num
    GOTO _RW
    _RW:
    RET_RS = RET_RV
    GOTO _RT
    _RU:
    RET_RS = num
    GOTO _RT
    _RT:
    RET_RP = RET_RS
    GOTO _RQ
    _RR:
    RET_RP = num
    GOTO _RQ
    _RQ:
    RET_RM = RET_RP
    GOTO _RN
    _RO:
    RET_RM = num
    GOTO _RN
    _RN:
    RET_RJ = RET_RM
    GOTO _RK
    _RL:
    RET_RJ = num
    GOTO _RK
    _RK:
    RET_RG = RET_RJ
    GOTO _RH
    _RI:
    RET_RG = num
    GOTO _RH
    _RH:
    RET_RD = RET_RG
    GOTO _RE
    _RF:
    RET_RD = num
    GOTO _RE
    _RE:
    RET_RA = RET_RD
    GOTO _RB
    _RC:
    RET_RA = num
    GOTO _RB
    _RB:
    RET_QX = RET_RA
    GOTO _QY
    _QZ:
    RET_QX = num
    GOTO _QY
    _QY:
    RET_QU = RET_QX
    GOTO _QV
    _QW:
    RET_QU = num
    GOTO _QV
    _QV:
    RET_QR = RET_QU
    GOTO _QS
    _QT:
    RET_QR = num
    GOTO _QS
    _QS:
    RET_QO = RET_QR
    GOTO _QP
    _QQ:
    RET_QO = num
    GOTO _QP
    _QP:
    RET_QL = RET_QO
    GOTO _QM
    _QN:
    RET_QL = num
    GOTO _QM
    _QM:
    RET_QI = RET_QL
    GOTO _QJ
    _QK:
    RET_QI = num
    GOTO _QJ
    _QJ:
    RET_QF = RET_QI
    GOTO _QG
    _QH:
    RET_QF = num
    GOTO _QG
    _QG:
    RET_QC = RET_QF
    GOTO _QD
    _QE:
    RET_QC = num
    GOTO _QD
    _QD:
    RET_PZ = RET_QC
    GOTO _QA
    _QB:
    RET_PZ = num
    GOTO _QA
    _QA:
    RET_PW = RET_PZ
    GOTO _PX
    _PY:
    RET_PW = num
    GOTO _PX
    _PX:
    RET_PT = RET_PW
    GOTO _PU
    _PV:
    RET_PT = num
    GOTO _PU
    _PU:
    RET_PQ = RET_PT
    GOTO _PR
    _PS:
    RET_PQ = num
    GOTO _PR
    _PR:
    RET_PN = RET_PQ
    GOTO _PO
    _PP:
    RET_PN = num
    GOTO _PO
    _PO:
    RET_PK = RET_PN
    GOTO _PL
    _PM:
    RET_PK = num
    GOTO _PL
    _PL:
    RET_PH = RET_PK
    GOTO _PI
    _PJ:
    RET_PH = num
    GOTO _PI
    _PI:
    RET_PE = RET_PH
    GOTO _PF
    _PG:
    RET_PE = num
    GOTO _PF
    _PF:
    RET_PB = RET_PE
    GOTO _PC
    _PD:
    RET_PB = num
    GOTO _PC
    _PC:
    RET_OY = RET_PB
    GOTO _OZ
    _PA:
    RET_OY = num
    GOTO _OZ
    _OZ:
    RET_OV = RET_OY
    GOTO _OW
    _OX:
    RET_OV = num
    GOTO _OW
    _OW:
    RET_OS = RET_OV
    GOTO _OT
    _OU:
    RET_OS = num
    GOTO _OT
    _OT:
    RET_OP = RET_OS
    GOTO _OQ
    _OR:
    RET_OP = num
    GOTO _OQ
    _OQ:
    RET_OM = RET_OP
    GOTO _ON
    _OO:
    RET_OM = num
    GOTO _ON
    _ON:
    RET_OJ = RET_OM
    GOTO _OK
    _OL:
    RET_OJ = num
    GOTO _OK
    _OK:
    RET_OG = RET_OJ
    GOTO _OH
    _OI:
    RET_OG = num
    GOTO _OH
    _OH:
    RET_OD = RET_OG
    GOTO _OE
    _OF:
    RET_OD = num
    GOTO _OE
    _OE:
    RET_OA = RET_OD
    GOTO _OB
    _OC:
    RET_OA = num
    GOTO _OB
    _OB:
    RET_NX = RET_OA
    GOTO _NY
    _NZ:
    RET_NX = num
    GOTO _NY
    _NY:
    RET_NU = RET_NX
    GOTO _NV
    _NW:
    RET_NU = num
    GOTO _NV
    _NV:
    RET_NR = RET_NU
    GOTO _NS
    _NT:
    RET_NR = num
    GOTO _NS
    _NS:
    RET_NO = RET_NR
    GOTO _NP
    _NQ:
    RET_NO = num
    GOTO _NP
    _NP:
    RET_NL = RET_NO
    GOTO _NM
    _NN:
    RET_NL = num
    GOTO _NM
    _NM:
    RET_NI = RET_NL
    GOTO _NJ
    _NK:
    RET_NI = num
    GOTO _NJ
    _NJ:
    RET_NF = RET_NI
    GOTO _NG
    _NH:
    RET_NF = num
    GOTO _NG
    _NG:
    RET_NC = RET_NF
    GOTO _ND
    _NE:
    RET_NC = num
    GOTO _ND
    _ND:
    RET_MZ = RET_NC
    GOTO _NA
    _NB:
    RET_MZ = num
    GOTO _NA
    _NA:
    RET_MW = RET_MZ
    GOTO _MX
    _MY:
    RET_MW = num
    GOTO _MX
    _MX:
    RET_MT = RET_MW
    GOTO _MU
    _MV:
    RET_MT = num
    GOTO _MU
    _MU:
    RET_MQ = RET_MT
    GOTO _MR
    _MS:
    RET_MQ = num
    GOTO _MR
    _MR:
    RET_MN = RET_MQ
    GOTO _MO
    _MP:
    RET_MN = num
    GOTO _MO
    _MO:
    RET_MK = RET_MN
    GOTO _ML
    _MM:
    RET_MK = num
    GOTO _ML
    _ML:
    RET_MH = RET_MK
    GOTO _MI
    _MJ:
    RET_MH = num
    GOTO _MI
    _MI:
    RET_ME = RET_MH
    GOTO _MF
    _MG:
    RET_ME = num
    GOTO _MF
    _MF:
    RET_MB = RET_ME
    GOTO _MC
    _MD:
    RET_MB = num
    GOTO _MC
    _MC:
    RET_LY = RET_MB
    GOTO _LZ
    _MA:
    RET_LY = num
    GOTO _LZ
    _LZ:
    RET_LV = RET_LY
    GOTO _LW
    _LX:
    RET_LV = num
    GOTO _LW
    _LW:
    RET_LS = RET_LV
    GOTO _LT
    _LU:
    RET_LS = num
    GOTO _LT
    _LT:
    RET_LP = RET_LS
    GOTO _LQ
    _LR:
    RET_LP = num
    GOTO _LQ
    _LQ:
    RET_LM = RET_LP
    GOTO _LN
    _LO:
    RET_LM = num
    GOTO _LN
    _LN:
    RET_LJ = RET_LM
    GOTO _LK
    _LL:
    RET_LJ = num
    GOTO _LK
    _LK:
    RET_LG = RET_LJ
    GOTO _LH
    _LI:
    RET_LG = num
    GOTO _LH
    _LH:
    RET_LD = RET_LG
    GOTO _LE
    _LF:
    RET_LD = num
    GOTO _LE
    _LE:
    RET_LA = RET_LD
    GOTO _LB
    _LC:
    RET_LA = num
    GOTO _LB
    _LB:
    RET_KX = RET_LA
    GOTO _KY
    _KZ:
    RET_KX = num
    GOTO _KY
    _KY:
    RET_KU = RET_KX
    GOTO _KV
    _KW:
    RET_KU = num
    GOTO _KV
    _KV:
    RET_KR = RET_KU
    GOTO _KS
    _KT:
    RET_KR = num
    GOTO _KS
    _KS:
    RET_KQ = RET_KR
    RETURN RET_KQ
    _KP:
    RETURN num
END FUNCTION";

            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod]
        public void BaZicOptimizerProgram()
        {
            var parser = new BaZicParser();
            var codeGenerator = new BaZicCodeGenerator();

            var inputCode =
@"

EXTERN FUNCTION Main(args[])
    MyFunction(1, 2, NULL)
END FUNCTION

VARIABLE myVar[] = NEW [""value1"", ""val2""]


FUNCTION MyFunction(arg1, arg2, arg3[])
    DO
        VARIABLE x = 1 + 2 * (3 + 4 + 5)
        x = myVar[0]
        x = ""hello"" + x.ToString()
        x = 1.ToString()
        BREAK
    LOOP WHILE myVar = arg1 OR(arg1 = arg2 AND arg2 = arg3[0])
    arg3 = NEW System.DateTime()
    RETURN RecursivityFunction(100)
END FUNCTION

ASYNC FUNCTION RecursivityFunction(num)
    IF num > 1 THEN
        num = AWAIT(RecursivityFunction(num – 1))
        TRY
            num.ToString() # this is a comment
        CATCH
            THROW NEW System.Exception(EXCEPTION.Message)
        END TRY
    ELSE
        IF NOT num = 1 THEN
            # another comment
        END IF
    END IF

    RETURN num
END FUNCTION

";

            var program = parser.Parse(inputCode, true).Program;
            var result = codeGenerator.Generate(program);

            var expectedResult =
@"# BaZic code generated automatically

VARIABLE myVar[] = NEW [""value1"", ""val2""]

EXTERN FUNCTION Main(args[])
    VARIABLE arg1 = 1
    VARIABLE arg2 = 2
    VARIABLE arg3[] = NULL
    VARIABLE RET_A
    _A:
    _C:
    VARIABLE x = 1 + (2 * ((3 + 4) + 5))
    x = myVar[0]
    x = ""hello"" + x.ToString()
    x = 1.ToString()
    GOTO _D
    IF NOT ((myVar = arg1) OR ((arg1 = arg2) AND (arg2 = arg3[0]))) GOTO _D
    GOTO _C
    _D:
    arg3 = NEW System.DateTime()
    RET_A = RecursivityFunction(100)
    GOTO _B
    _B:
END FUNCTION

FUNCTION MyFunction(arg1, arg2, arg3[])
    _E:
    VARIABLE x = 1 + (2 * ((3 + 4) + 5))
    x = myVar[0]
    x = ""hello"" + x.ToString()
    x = 1.ToString()
    GOTO _F
    IF NOT ((myVar = arg1) OR ((arg1 = arg2) AND (arg2 = arg3[0]))) GOTO _F
    GOTO _E
    _F:
    arg3 = NEW System.DateTime()
    VARIABLE RET_G
    RET_G = RecursivityFunction(100)
    RETURN RET_G
END FUNCTION

ASYNC FUNCTION RecursivityFunction(num)
    IF NOT (num >= 1) GOTO _H
    num = AWAIT RecursivityFunction(num - 1)
    TRY
        num.ToString()
    CATCH
        THROW NEW System.Exception(EXCEPTION.Message)
    END TRY
    GOTO _I
    _H:
    IF NOT (NOT (num = 1)) GOTO _J
    _J:
    _I:
    RETURN num
END FUNCTION";

            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod]
        public void BaZicOptimizerTryCatch()
        {
            var parser = new BaZicParser();
            var codeGenerator = new BaZicCodeGenerator();

            var inputCode =
@"
EXTERN FUNCTION Main(args[])
    RETURN Method1()
END FUNCTION

FUNCTION Method1()
    DO WHILE True
        TRY
            IF TRUE THEN
                BREAK
            END IF
        CATCH
            IF TRUE THEN
                RETURN ""FOO""
            END IF
        END TRY
    LOOP

    RETURN ""Hello""
END FUNCTION";

            var program = parser.Parse(inputCode, true).Program;
            var result = codeGenerator.Generate(program);

            var expectedResult =
@"# BaZic code generated automatically

EXTERN FUNCTION Main(args[])
    VARIABLE RET_A
    VARIABLE RET_B
    _B:
    _D:
    IF NOT TRUE GOTO _E
    TRY
        IF NOT TRUE GOTO _F
        GOTO _E
        _F:
    CATCH
        IF NOT TRUE GOTO _G
        RET_B = ""FOO""
        GOTO _C
        _G:
    END TRY
    GOTO _D
    _E:
    RET_B = ""Hello""
    GOTO _C
    _C:
    RET_A = RET_B
    RETURN RET_A
END FUNCTION

FUNCTION Method1()
    _H:
    IF NOT TRUE GOTO _I
    TRY
        IF NOT TRUE GOTO _J
        GOTO _I
        _J:
    CATCH
        IF NOT TRUE GOTO _K
        RETURN ""FOO""
        _K:
    END TRY
    GOTO _H
    _I:
    RETURN ""Hello""
END FUNCTION";

            Assert.AreEqual(expectedResult, result);
        }
    }
}
