namespace Codeflows.Metrics.Csharp.Common.Util
{
    public record DirectoryMetadata(int NumberOfFiles, long SizeInBytes);

    public static class DirectoryUtils
    {
        public static DirectoryMetadata GetDirectoryMetadata(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                throw new InvalidOperationException("Directory at provided path does not exist");
            }

            long sizeInBytes = 0;

            var directoryInfo = new DirectoryInfo(directoryPath);
            var fileInfos = directoryInfo.GetFiles();
            int numberOfFiles = fileInfos.Length;

            foreach (var fileInfo in fileInfos)
            {
                sizeInBytes += fileInfo.Length;
            }

            var subdirectoryInfos = directoryInfo.GetDirectories();

            foreach (var dirInfo in subdirectoryInfos)
            {
                var metadata = GetDirectoryMetadata(dirInfo.FullName);
                sizeInBytes += metadata.SizeInBytes;
                numberOfFiles += metadata.NumberOfFiles;
            }

            return new DirectoryMetadata(numberOfFiles, sizeInBytes);
        }
    }
}
