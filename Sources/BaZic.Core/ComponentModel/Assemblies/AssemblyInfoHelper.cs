using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace BaZic.Core.ComponentModel.Assemblies
{
    public static class AssemblyInfoHelper
    {
        /// <summary>
        /// Gets a new instance of <see cref="AssemblyDetails"/> filled from the full name or the location of an assembly.
        /// </summary>
        /// <param name="assemblyName">Defines the full name or the location of the assembly.</param>
        /// <returns>An instance of <see cref="AssemblyDetails"/></returns>
        public static AssemblyDetails GetAssemblyDetailsFromName(string assemblyName)
        {
            var details = new AssemblyDetails();
            var fullName = string.Empty;

            if (File.Exists(assemblyName))
            {
                if (IsDotNetAssembly(assemblyName))
                {
                    var assemblyNameInfo = AssemblyName.GetAssemblyName(assemblyName);
                    details.Location = assemblyName;
                    details.CopyToLocal = true;

                    fullName = assemblyNameInfo.FullName;
                }
                else
                {
                    // TODO : Still try to retrieve data.
                    throw new BadImageFormatException($"The file '{assemblyName}' is not a .NET Assembly.");
                }
            }
            else
            {
                fullName = assemblyName;
            }

            var properties = fullName.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);

            var name = properties.FirstOrDefault();
            var version = properties.SingleOrDefault(p => p.StartsWith(Consts.AssemblyPropertyVersion))?.Replace(Consts.AssemblyPropertyVersion, string.Empty);
            var culture = properties.SingleOrDefault(p => p.StartsWith(Consts.AssemblyPropertyCulture))?.Replace(Consts.AssemblyPropertyCulture, string.Empty);
            var publicKeyToken = properties.SingleOrDefault(p => p.StartsWith(Consts.AssemblyPropertyPublicKeyToken))?.Replace(Consts.AssemblyPropertyPublicKeyToken, string.Empty);
            var processorArchitecture = properties.SingleOrDefault(p => p.StartsWith(Consts.AssemblyPropertyProcessorArchitecture))?.Replace(Consts.AssemblyPropertyProcessorArchitecture, string.Empty);
            var custom = properties.SingleOrDefault(p => p.StartsWith(Consts.AssemblyPropertyCustom))?.Replace(Consts.AssemblyPropertyCustom, string.Empty);

            var processorArch = ProcessorArchitecture.None;
            switch (processorArchitecture)
            {
                case Consts.AssemblyPropertyX86:
                    processorArch = ProcessorArchitecture.X86;
                    break;

                case Consts.AssemblyPropertyX64:
                    processorArch = ProcessorArchitecture.Amd64;
                    break;

                case Consts.AssemblyPropertyAnyCPU:
                    processorArch = ProcessorArchitecture.MSIL;
                    break;

                default:
                    break;
            }

            details.Culture = culture;
            details.Custom = custom;
            details.FullName = fullName;
            details.Name = name;
            details.ProcessorArchitecture = processorArch;
            details.PublicKeyToken = publicKeyToken;
            details.Version = version;

            return details;
        }

        /// <summary>
        /// Determines whether the specified file is a .NET assembly or not. Credit to Kirill Osenkov : https://stackoverflow.com/questions/367761/how-to-determine-whether-a-dll-is-a-managed-assembly-or-native-prevent-loading
        /// </summary>
        /// <param name="filePath">The path to the file</param>
        /// <returns>Returns <c>True</c> if the file is a .NET assembly.</returns>
        public static bool IsDotNetAssembly(string filePath)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                if (fileStream.Length < 64)
                {
                    return false;
                }

                //PE Header starts @ 0x3C (60). Its a 4 byte header.
                fileStream.Position = 0x3C;
                var peHeaderPointer = binaryReader.ReadUInt32();
                if (peHeaderPointer == 0)
                {
                    peHeaderPointer = 0x80;
                }

                // Ensure there is at least enough room for the following structures:
                //     24 byte PE Signature & Header
                //     28 byte Standard Fields         (24 bytes for PE32+)
                //     68 byte NT Fields               (88 bytes for PE32+)
                // >= 128 byte Data Dictionary Table
                if (peHeaderPointer > fileStream.Length - 256)
                {
                    return false;
                }

                // Check the PE signature.  Should equal 'PE\0\0'.
                fileStream.Position = peHeaderPointer;
                var peHeaderSignature = binaryReader.ReadUInt32();
                if (peHeaderSignature != 0x00004550)
                {
                    return false;
                }

                // skip over the PEHeader fields
                fileStream.Position += 20;

                const ushort PE32 = 0x10b;
                const ushort PE32Plus = 0x20b;

                // Read PE magic number from Standard Fields to determine format.
                var peFormat = binaryReader.ReadUInt16();
                if (peFormat != PE32 && peFormat != PE32Plus)
                {
                    return false;
                }

                // Read the 15th Data Dictionary RVA field which contains the CLI header RVA.
                // When this is non-zero then the file contains CLI data otherwise not.
                var dataDictionaryStart = (ushort)(peHeaderPointer + (peFormat == PE32 ? 232 : 248));
                fileStream.Position = dataDictionaryStart;

                var cliHeaderRva = binaryReader.ReadUInt32();
                if (cliHeaderRva == 0)
                {
                    return false;
                }

                return true;
            }
        }
    }
}
