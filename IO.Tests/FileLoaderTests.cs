using IO;
using Xunit;

namespace IO.Tests;
public class FileLoaderTests
{
    [Fact]
    public void Throws_When_File_Not_Found()
    {
        var loader = new FileLoader();
        Assert.Throws<FileNotFoundException>(() => loader.Load("nope.txt"));
    }
}
