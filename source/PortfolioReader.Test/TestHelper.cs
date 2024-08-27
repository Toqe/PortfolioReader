namespace Toqe.PortfolioReader.Test
{
    public class TestHelper
    {
        public DirectoryInfo GetTestDataBasePath()
        {
            var basePath = new DirectoryInfo(".");

            while (basePath.Name != "source")
            {
                basePath = basePath.Parent;

                if (basePath == null)
                {
                    throw new InvalidOperationException("Base path not found");
                }
            }

            return new DirectoryInfo(Path.Combine(basePath.FullName, "PortfolioReader.Test", "TestData"));
        }
    }
}
