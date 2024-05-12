using System.Diagnostics;
using Bakabase.Tool.FixFileMoverPadding;

Console.WriteLine("欢迎使用Bakabase/InsideWorld文件移动器文件异常填充修复器。");
Console.WriteLine();
Console.WriteLine(
    "该修复器主要用于修复在使用了Bakabase/InsideWorld(版本小于等于v1.8.0-beta)的文件移动工具时，如果进行了跨磁盘移动文件，则会将文件向上填充至1mb的整数倍。具体情况可以通过 https://github.com/anobaka/InsideWorld/issues/500 查看。");
Console.WriteLine();
Console.WriteLine("如您未使用文件移动工具或未出现跨磁盘移动的情况，请勿随意使用本工具。");
Console.WriteLine();
Console.WriteLine("本工具将会扫描当前目录下所有因为上述问题产生的异常文件，并创建一个新的目录，用于保存所有已修复的文件，这些文件的目录级别会和当前目录保持一致。请确保您有足够的剩余空间用于保存已修复的文件。");
Console.WriteLine(@"本工具不会删除任何文件，您可以在修复完成后预览这些已修复的文件，如果确认无误后，可以批量将其覆盖至当前目录。");
Console.WriteLine("---------------------------------------------------------------------------------");
Console.WriteLine("以下是本次修复的基础信息：");

await File.WriteAllTextAsync(@"C:\Users\anoba\Downloads\small.txt",
    BitConverter.ToString(File.ReadAllBytes(@"E:\迅雷下载\[EBA] ラブホイール 媚薬に狂う姉妹 [中国翻訳] [DL版][裸單騎漢化+精英牛頭人漢化]\237.png")));

try
{
    // var rootDir = "Y:\\X";
    var rootDir = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar,
    Path.VolumeSeparatorChar);
    Console.WriteLine($"扫描目录：{rootDir}");
    var outputDir = Path.Combine(rootDir, $"Bakabase.Tool.FixFileMoverPadding.Fixed").TrimEnd(
        Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar,
        Path.VolumeSeparatorChar);
    Console.WriteLine($"修复文件保存目录：{outputDir}");
    Console.WriteLine("---------------------------------------------------------------------------------");
    while (true)
    {
        Console.Write("请确认以上信息，如需开始修复请输入y并按下回车：");
        var x = Console.ReadLine();
        if (x?.ToLower().Trim() == "y")
        {
            break;
        }
        else
        {
            Console.WriteLine("输入有误。");
        }
    }

    Console.WriteLine("正在修复中，请稍后。。。");
    Directory.CreateDirectory(outputDir);
    var appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
    var result = new Repairman.FileMoverPaddingBugFixResult();
    await Repairman.FixFiles(rootDir, result, [outputDir, appPath], rootDir, outputDir);

    Process.Start("explorer.exe", outputDir);
    Console.WriteLine($"修复完成，{result}");
    Console.WriteLine("已自动打开修复文件目录，如需替换请直接将其覆盖至原目录。");
}
catch (Exception e)
{
    Console.WriteLine("发生意外情况，请将以下内容复制发送给开发者。");
    Console.WriteLine(e.Message);
    Console.WriteLine(e.StackTrace);
}

Console.WriteLine("按任意键退出");
Console.Read();