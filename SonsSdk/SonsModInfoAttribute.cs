using MelonLoader;
using Semver;

namespace SonsSdk;

[AttributeUsage(AttributeTargets.Assembly)]
public class SonsModInfoAttribute : MelonInfoAttribute
{
        /// <summary>
        /// SonsMod constructor.
        /// </summary>
        /// <param name="type">The main Melon type of the Melon (for example TestMod)</param>
        /// <param name="name">Name of the Melon</param>
        /// <param name="version">Version of the Melon</param>
        /// <param name="author">Author of the Melon</param>
        /// <param name="downloadLink">URL to the download link of the mod [optional]</param>
        public SonsModInfoAttribute(Type type, string name, string version, string author, string downloadLink = null) 
            : base(type, name, version, author, downloadLink)
        { }

        /// <summary>
        /// SonsMod constructor.
        /// </summary>
        /// <param name="type">The main Melon type of the Melon (for example TestMod)</param>
        /// <param name="name">Name of the Melon</param>
        /// <param name="versionMajor">Version Major of the Melon (Using the <see href="https://semver.org">Semantic Versioning</see> format)</param>
        /// <param name="versionMinor">Version Minor of the Melon (Using the <see href="https://semver.org">Semantic Versioning</see> format)</param>
        /// <param name="versionRevision">Version Revision of the Melon (Using the <see href="https://semver.org">Semantic Versioning</see> format)</param>
        /// <param name="versionIdentifier">Version Identifier of the Melon (Using the <see href="https://semver.org">Semantic Versioning</see> format)</param>
        /// <param name="author">Author of the Melon</param>
        /// <param name="downloadLink">URL to the download link of the mod [optional]</param>
        public SonsModInfoAttribute(Type type, string name, int versionMajor, int versionMinor, int versionRevision, string versionIdentifier, string author, string downloadLink = null)
            : base(type, name, versionMajor, versionMinor, versionRevision, versionIdentifier, author, downloadLink)
        {}

        /// <summary>
        /// SonsMod constructor.
        /// </summary>
        /// <param name="type">The main Melon type of the Melon (for example TestMod)</param>
        /// <param name="name">Name of the Melon</param>
        /// <param name="versionMajor">Version Major of the Melon (Using the <see href="https://semver.org">Semantic Versioning</see> format)</param>
        /// <param name="versionMinor">Version Minor of the Melon (Using the <see href="https://semver.org">Semantic Versioning</see> format)</param>
        /// <param name="versionRevision">Version Revision of the Melon (Using the <see href="https://semver.org">Semantic Versioning</see> format)</param>
        /// <param name="author">Author of the Melon</param>
        /// <param name="downloadLink">URL to the download link of the mod [optional]</param>
        public SonsModInfoAttribute(Type type, string name, int versionMajor, int versionMinor, int versionRevision, string author, string downloadLink = null)
            : base(type, name, versionMajor, versionMinor, versionRevision, author, downloadLink)
        {}
}