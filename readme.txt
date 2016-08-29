nTsMapper Installation

Once you've installed the nTsMapper nuget package, in order for it to work, you MUST point 
your project to a dll that contains the C# classes you want mapped to TypeScript classes. To 
do this, you need to modify the following XML and add it to your project file:

<PropertyGroup>
	<nTsMapperInput>$(SolutionDir)Path\To\Your\CSharp.Class.Library.dll</nTsMapperInput>
</PropertyGroup>

This will cause the mapper to look for a CSharp.Class.Library.dll in that path.

NOTE: You can also customize where your resulting TypeScript file is placed and what it's 
called by adding another element to the XML, like in this example:

<PropertyGroup>
	<nTsMapperInput>$(SolutionDir)Path\To\Your\CSharp.Class.Library.dll</nTsMapperInput>
	<nTsMapperOutput>$(ProjectDir)Place\You\Want\To\Put\The.Mapped.TypeScript.File.ts</nTsMapperOutput>
</PropertyGroup>