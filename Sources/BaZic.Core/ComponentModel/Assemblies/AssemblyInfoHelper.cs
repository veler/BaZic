﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BaZic.Core.ComponentModel.Assemblies
{
    public static class AssemblyInfoHelper
    {
        /// <summary>
        /// Gets a new instance of <see cref="AssemblyDetails"/> filled from the full name or the location of an assembly.
        /// </summary>
        /// <param name="assemblyNameOrLocation">Defines the full name or the location of the assembly.</param>
        /// <returns>An instance of <see cref="AssemblyDetails"/></returns>
        public static AssemblyDetails GetAssemblyDetailsFromNameOrLocation(string assemblyNameOrLocation)
        {
            var details = new AssemblyDetails();
            var fullName = string.Empty;

            if (File.Exists(assemblyNameOrLocation))
            {
                details.Location = assemblyNameOrLocation;
                if (IsDotNetAssembly(assemblyNameOrLocation))
                {
                    var assemblyNameInfo = AssemblyName.GetAssemblyName(assemblyNameOrLocation);
                    details.CopyToLocal = true;
                    details.IsDotNetAssembly = true;
                    fullName = assemblyNameInfo.FullName;
                }
                else
                {
                    var windowsPath = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
                    if (!details.Location.StartsWith(windowsPath))
                    {
                        details.CopyToLocal = true;
                    }

                    var fileVersionInfo = FileVersionInfo.GetVersionInfo(details.Location);
                    fullName = $"{fileVersionInfo.FileDescription}, Version={fileVersionInfo.FileVersion}, Culture={fileVersionInfo.Language}";
                }
            }
            else
            {
                details.IsDotNetAssembly = true;
                fullName = assemblyNameOrLocation;
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
            {
                return IsDotNetAssembly(fileStream);
            }
        }

        /// <summary>
        /// Determines whether the specified file is a .NET assembly or not. Credit to Kirill Osenkov : https://stackoverflow.com/questions/367761/how-to-determine-whether-a-dll-is-a-managed-assembly-or-native-prevent-loading
        /// </summary>
        /// <param name="assemblyStream">A stream that contains the file assembly.</param>
        /// <returns>Returns <c>True</c> if the file is a .NET assembly.</returns>
        public static bool IsDotNetAssembly(Stream assemblyStream)
        {
            using (var binaryReader = new BinaryReader(assemblyStream, Encoding.Default, true))
            {
                if (assemblyStream.Length < 64)
                {
                    return false;
                }

                //PE Header starts @ 0x3C (60). Its a 4 byte header.
                assemblyStream.Position = 0x3C;
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
                if (peHeaderPointer > assemblyStream.Length - 256)
                {
                    return false;
                }

                // Check the PE signature.  Should equal 'PE\0\0'.
                assemblyStream.Position = peHeaderPointer;
                var peHeaderSignature = binaryReader.ReadUInt32();
                if (peHeaderSignature != 0x00004550)
                {
                    return false;
                }

                // skip over the PEHeader fields
                assemblyStream.Position += 20;

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
                assemblyStream.Position = dataDictionaryStart;

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
