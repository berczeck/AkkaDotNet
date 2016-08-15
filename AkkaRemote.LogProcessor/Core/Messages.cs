using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class ProcessLogFolder
    {
        public string Path { get; }
        public ProcessLogFolder(string path)
        {
            Path = path;
        }
    }

    public class ProcessFile
    {
        public string Path { get; }
        public ProcessFile(string path)
        {
            Path = path;
        }
    }

    public class LogFolderProcessed
    {
        public string Path { get; }
        public int NumberOfFiles { get; }
        public TimeSpan ExecutionTime { get; set; }
        public LogFolderProcessed(string path, int numberOfFiles, TimeSpan executionTime)
        {
            Path = path;
            NumberOfFiles = numberOfFiles;
            ExecutionTime = executionTime;
        }
    }

    public class LogFolderFailed
    {
        public string Path { get; }
        public LogFolderFailed(string path)
        {
            Path = path;
        }
    }

    public class LogFileProcessed
    {
        public string Path { get; }
        public LogFileProcessed(string path)
        {
            Path = path;
        }
    }
}
