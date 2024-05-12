using System.Diagnostics;

namespace Bakabase.Tool.FixFileMoverPadding;

public class Repairman
{
    public record FileMoverPaddingBugFixResult
    {
        public int TotalFileCount { get; set; }
        public int BadFileCount { get; set; }
        public long SizeDiff { get; set; }
        public decimal SizeDiffInMb => Math.Round((decimal) SizeDiff / 1024 / 1024, 2);
        public Stopwatch StopWatch { get; set; } = Stopwatch.StartNew();

        public override string ToString()
        {
            return $"已扫描: {TotalFileCount}, 已修复: {BadFileCount}, 减少占用空间: {SizeDiffInMb:F2}mb, 耗时: {StopWatch.Elapsed}";
        }
    }

    public static async Task FixFiles(string currentDir, FileMoverPaddingBugFixResult result,
        HashSet<string> excludedPaths, string rootDir, string outputDir)
    {
        var files = Directory.GetFiles(currentDir).ToList();

        foreach (var file in files)
        {
            if (excludedPaths.Contains(file))
            {
                continue;
            }

            result.TotalFileCount++;
            if (result.TotalFileCount % 500 == 0)
            {
                Console.WriteLine($"\r{result}");

                var fi = new FileInfo(file);
                var fl = fi.Length;
                const int segmentLength = 1024 * 1024;
                if (fl % segmentLength == 0)
                {
                    var outputFile = file.Replace(rootDir, outputDir);
                    if (File.Exists(outputFile))
                    {
                        continue;
                    }

                    try
                    {
                        byte[] fixedData;
                        var segmentCount = (int) (fl / segmentLength);
                        var fs = fi.OpenRead();
                        var fixStartIdx = Math.Max(segmentCount - 2, 0) * (long)segmentLength;
                        fs.Seek(fixStartIdx, SeekOrigin.Begin);
                        var badData = new byte[fl - fixStartIdx];
                        _ = await fs.ReadAsync(badData, 0, badData.Length);
                        if (segmentCount > 1)
                        {
                            var lastSegment = badData.Skip(segmentLength).ToList();
                            var segmentBeforeLast = badData.Take(segmentLength).ToList();
                            var idx = -1;
                            for (var i = segmentLength - 1; i >= 0; i--)
                            {
                                if (lastSegment[i] != segmentBeforeLast[i])
                                {
                                    idx = i;
                                    break;
                                }
                            }

                            if (idx == -1)
                            {
                                break;
                            }

                            fixedData = badData.Take(segmentLength + idx + 1).ToArray();
                        }
                        else
                        {
                            var idx = -1;
                            for (var i = badData.Length - 1; i >= 0; i--)
                            {
                                if (badData[i] != 0)
                                {
                                    idx = i;
                                    break;
                                }
                            }

                            if (idx == -1)
                            {
                                break;
                            }

                            fixedData = badData.Take(idx + 1).ToArray();
                        }

                        if (fixedData.Length != badData.Length)
                        {
                            var dir = Path.GetDirectoryName(outputFile)!;
                            Directory.CreateDirectory(dir);

                            var newFs = File.OpenWrite(outputFile);
                            var writeBuffer = new byte[segmentLength];
                            fs.Seek(0, SeekOrigin.Begin);
                            while (fs.Position < fixStartIdx)
                            {
                                var count = await fs.ReadAsync(writeBuffer, 0, segmentLength);
                                await newFs.WriteAsync(writeBuffer, 0, count);
                            }

                            await newFs.WriteAsync(fixedData, 0, fixedData.Length);
                            newFs.Close();

                            result.BadFileCount++;
                            result.SizeDiff += badData.Length - fixedData.Length;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"针对文件：{file}，发生了一个异常：{e.Message}");
                        Console.WriteLine(e.StackTrace);
                    }
                }
            }

            var dirs = Directory.GetDirectories(currentDir);
            foreach (var d in dirs)
            {
                if (excludedPaths.Contains(d))
                {
                    continue;
                }

                await FixFiles(d, result, excludedPaths, rootDir, outputDir);
            }
        }
    }
}