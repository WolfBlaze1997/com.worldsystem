#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;
using UnityEditor;
using UnityEditor.Callbacks;
using System.IO;
using System.Linq;
using System.Net.Mime;
using Sirenix.OdinInspector;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine.Serialization;
using Directory = System.IO.Directory;
using File = System.IO.File;

namespace WorldSystem.Runtime
{
    
    [ExecuteAlways]
    public class PackageManager : BaseModule
    {
        
        [LabelText("包管理器")]
        [ListDrawerSettings(HideRemoveButton = true)]
        [TableList(AlwaysExpanded = true, DrawScrollView = false, HideToolbar = true)]
        public PackageJson[] PJ;
        
        [HideInInspector]public string[] Paths;
        
        [Serializable]
        public class PackageJson
        {
            [Space(20)]
            [VerticalGroup("启用")]
            [LabelText(" ")] [TableColumnWidth(55, Resizable = false)]
            public bool active;
            
            [Space(3)]
            [VerticalGroup("图标")][HideLabel]
            [PreviewField(Alignment = ObjectFieldAlignment.Center)] [TableColumnWidth(57, Resizable = false)]
            [EnableIf("@false")]
            public Texture2D icon;
            
            [Space(3)]
            [VerticalGroup("信息")][LabelText("名字")] [TableColumnWidth(180, Resizable = false)]
            [EnableIf("@false")]
            public string name;
            
            [Space(-3)]
            [VerticalGroup("信息")][LabelText("开发者")] 
            [EnableIf("@false")]
            public string creator;
            
            [Space(-3)]
            [VerticalGroup("信息")][LabelText("版本")] 
            [EnableIf("@false")]
            public string version;
            
            [VerticalGroup("描述")][HideLabel][Multiline(3)][Space(3)]
            [EnableIf("@false")]
            public string describe;
            
            [HideInInspector]
            public string[] patchFilePaths;

        }

        private string PackagesPath;
        private string PluginsPath;
        
        
        private void OnEnable()
        {
            if (Application.isPlaying) return;
            //获取包路径与插件路径
            PackagesPath = UnityEngine.Windows.Directory.localFolder.Replace("LocalState", "Packages/com.worldsystem/Packages~").Replace("\\","/");  
            PluginsPath = UnityEngine.Windows.Directory.localFolder.Replace("LocalState", "Packages/com.worldsystem/Assets/Plugins").Replace("\\","/");
            
            //获取所有包的路径
            Paths = Directory.GetDirectories(PackagesPath);
            //初始化PJ
            PJ = new PackageJson[Paths.Length];
            
            for (var index = 0; index < Paths.Length; index++)
            {
                //反序列化包的SimplePackage.json
                string PackageJson = File.ReadAllText(Paths[index] + "/SimplePackage.json");
                PJ[index] = JsonUtility.FromJson<PackageJson>(PackageJson);
                //处理包的图标
                if (File.Exists(Paths[index] + "/Icon.png"))
                {
                    byte[] request = File.ReadAllBytes(Paths[index] + "/Icon.png");
                    Texture2D texture = new Texture2D(64,64);
                    texture.LoadImage(request);
                    PJ[index].icon = texture;
                }
                else
                {
                    PJ[index].icon = Texture2D.blackTexture;
                }
            }
            OnValidate();
        }
        
        
        
        private void OnValidate()
        {
            if (Application.isPlaying) return;

            if (Paths.Length == 0 || PJ.Length == 0) return;
            for (var index = 0; index < PJ.Length; index++)
            {
                //如果更改了参数,重新将其写入Json
                
                //文件夹名字
                string directoryName = Paths[index].Split("\\").Last();
                //包根目录下的SimplePackage.json文件
                string simplePackageJson = Paths[index] + "/SimplePackage.json";

                //对应的包路径
                string packagesDirectory = PackagesPath + "/"+ directoryName;
                //包文件夹的Meta文件
                string packagesDirectoryMeta = PackagesPath + "/"+ directoryName + ".meta";
                
                //对应的插件路径
                string pluginsDirectory = PluginsPath + "/" + directoryName;
                //插件文件夹的Meta文件
                string pluginsDirectoryMeta = PluginsPath + "/"+ directoryName + ".meta";
                
                RefreshJsonFile(index, simplePackageJson);

                if (PJ[index].active)
                {
                    //如果激活但不存在插件文件夹,则将其复制到插件文件夹
                    if (!Directory.Exists(pluginsDirectory) )
                    {
                        //复制包文件夹到插件文件夹内
                        CopyFileAndDir(packagesDirectory, pluginsDirectory);
                        
                        if (Directory.Exists(pluginsDirectory + "/Patch"))
                        {
                            //将包中的补丁拷贝到目标文件夹
                            string[] strings = Directory.GetDirectories(pluginsDirectory + "/Patch");
                            foreach (var VARIABLE in strings)
                            {
                                CopyFileAndDir(VARIABLE, PluginsPath + "/" +  VARIABLE.Split("\\").Last());
                            }

                            //获取Patch文件夹内(包括子文件夹)的所有文件的路径
                            string[] files = Directory.GetFiles(pluginsDirectory + "/Patch", "*", SearchOption.AllDirectories);
                            for (int i = 0; i < files.Length; i++)
                            {
                                files[i] = files[i].Replace("\\", "/").Replace(pluginsDirectory + "/Patch", PluginsPath);
                                // Debug.Log(files[i]);
                            }
                            PJ[index].patchFilePaths = files;
                            
                            RefreshJsonFile(index, simplePackageJson);
                            
                            //拷贝之后将补丁文件夹删除
                            DeleteDirectory(pluginsDirectory + "/Patch");
                        }
                        //如果包文件夹存在Meta数据,则同样复制
                        if(File.Exists(packagesDirectoryMeta))
                            File.Copy(packagesDirectoryMeta,pluginsDirectoryMeta );
                    }
                }
                else
                {
                    //如果未激活但存在插件文件夹,则将其全部删除
                    if (Directory.Exists( pluginsDirectory))
                    {
                        Directory.Delete(pluginsDirectory, true);
                        if(File.Exists(pluginsDirectoryMeta))
                            File.Delete(pluginsDirectoryMeta);

                        if (PJ[index].patchFilePaths.Length != 0)
                        {
                            foreach (var VARIABLE in PJ[index].patchFilePaths)
                            {
                                if(File.Exists(VARIABLE))
                                    File.Delete(VARIABLE);
                                if (File.Exists(VARIABLE + ".meta"))
                                    File.Delete(VARIABLE + ".meta");
                            }
                        }
                            
                    }
                }
            }
            AssetDatabase.Refresh();
        }

        private void RefreshJsonFile(int index, string simplePackageJson)
        {
            //PJ序列化为Json
            string PJstring = JsonUtility.ToJson(PJ[index]).Replace("\",","\",\n");
            //更新包根目录下的SimplePackage.json文件
            File.WriteAllText(simplePackageJson, PJstring);
            //重新反序列化为PJ
            PJstring = File.ReadAllText(simplePackageJson);
            PJ[index] = JsonUtility.FromJson<PackageJson>(PJstring);
        }


        public static void DeleteDirectory(string targetDir)
        {
            string[] files = Directory.GetFiles(targetDir);
            string[] dirs = Directory.GetDirectories(targetDir);
 
            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }
 
            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }
 
            Directory.Delete(targetDir, true);
        }
        
        /// <summary>
        /// 复制文件夹下的所有文件、目录到指定的文件夹
        /// </summary>
        /// <param name="dir">源文件夹地址</param>
        /// <param name="desDir">指定的文件夹地址</param>
        public static void CopyFileAndDir(string dir, string desDir)
        {
            if (!Directory.Exists(desDir))
            {
                Directory.CreateDirectory(desDir);
            }
            
            IEnumerable<string> files = Directory.EnumerateFileSystemEntries(dir);
            var enumerable = files as string[] ?? files.ToArray();
            if (enumerable.Any())
            {
                foreach (var item in enumerable)
                {
                    string desPath = Path.Combine(desDir, Path.GetFileName(item));

                    //如果是文件
                    var fileExist = File.Exists(item);
                    if (fileExist)
                    {
                        //复制文件到指定目录下                     
                        File.Copy(item, desPath, true);
                        continue;
                    }

                    //如果是文件夹                   
                    CopyFileAndDir(item, desPath);

                }
            }
        }
    }
}

#endif