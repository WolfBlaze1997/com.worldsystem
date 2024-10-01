#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector;
using Directory = System.IO.Directory;
using File = System.IO.File;

namespace WorldSystem.Runtime
{
    
    [ExecuteAlways]
    public class PackageManager : BaseModule
    {

        #region 字段
        
        [PropertyOrder(-100)]
        [ShowIf("@UnityEditor.PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup).Contains(\"WB_LANGUAGE_CHINESE\")")]
        [Button(ButtonSizes.Large, Name = "语言设置: 中文"), GUIColor(0.5f, 0.5f, 1f)]
        private void SinicizationToggle_Off()
        {
            Language.Language.RemoveDefineSymbol();
        }
        
        [PropertyOrder(-100)]
        [HideIf("@UnityEditor.PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup).Contains(\"WB_LANGUAGE_CHINESE\")")]
        [Button(ButtonSizes.Large, Name = "Language Settings: English"), GUIColor(0.5f, 0.2f, 0.2f)]
        private void SinicizationToggle_On()
        {
            Language.Language.AddDefineSymbol();
        }
        
        [LabelText("包管理器")] [ListDrawerSettings(HideRemoveButton = true)] [TableList(AlwaysExpanded = true, DrawScrollView = false, HideToolbar = true)]
        public PackageJson[] pj;
        
        [HideInInspector]
        public List<string> paths;
        
        [Serializable]
        public class PackageJson
        {
            [Space(20)] [VerticalGroup("启用")] [LabelText(" ")] [TableColumnWidth(55, Resizable = false)]
            public bool active;
            
            [Space(3)] [VerticalGroup("图标")][HideLabel] [PreviewField(Alignment = ObjectFieldAlignment.Center)] [TableColumnWidth(57, Resizable = false)] [EnableIf("@false")]
            public Texture2D icon;
            
            [Space(3)] [VerticalGroup("信息")][LabelText("名字")] [TableColumnWidth(180, Resizable = false)] [EnableIf("@false")]
            public string name;
            
            [Space(-3)] [VerticalGroup("信息")][LabelText("开发者")] [EnableIf("@false")]
            public string creator;
            
            [Space(-3)] [VerticalGroup("信息")][LabelText("版本")] [EnableIf("@false")]
            public string version;
            
            [VerticalGroup("描述")][HideLabel][Multiline(3)][Space(3)] [EnableIf("@false")]
            public string describe;
            
            [HideInInspector]
            public string[] patchFilePaths;

        }

        private string _packagesPath;
        
        private string _pluginsPath;
        
        
        #endregion


        
        #region 事件函数
        
        private void OnEnable()
        {
            if (Application.isPlaying) return;
            //获取包路径与插件路径
            _packagesPath = UnityEngine.Windows.Directory.localFolder.Replace("LocalState", "Packages/com.worldsystem/Packages~").Replace("\\","/");  
            _pluginsPath = UnityEngine.Windows.Directory.localFolder.Replace("LocalState", "Packages/com.worldsystem/Assets/Plugins").Replace("\\","/");
            
            //获取所有包的路径
            paths = Directory.GetDirectories(_packagesPath).ToList();
            paths.Remove(paths.Find(o => o.Contains("Library")));
            //初始化PJ
            pj = new PackageJson[paths.Count];
            
            for (var index = 0; index < paths.Count; index++)
            {
                //反序列化包的SimplePackage.json
                string PackageJson = File.ReadAllText(paths[index] + "/SimplePackage.json");
                pj[index] = JsonUtility.FromJson<PackageJson>(PackageJson);
                //处理包的图标
                if (File.Exists(paths[index] + "/Icon.png"))
                {
                    byte[] request = File.ReadAllBytes(paths[index] + "/Icon.png");
                    Texture2D texture = new Texture2D(64,64);
                    texture.LoadImage(request);
                    pj[index].icon = texture;
                }
                else
                {
                    pj[index].icon = Texture2D.blackTexture;
                }
            }
            OnValidate();
        }
        
        private void OnValidate()
        {
            if (Application.isPlaying) return;

            if (paths.Count == 0 || pj.Length == 0) return;
            for (var index = 0; index < pj.Length; index++)
            {
                //如果更改了参数,重新将其写入Json
                
                //文件夹名字
                string directoryName = paths[index].Split("\\").Last();
                //包根目录下的SimplePackage.json文件
                string simplePackageJson = paths[index] + "/SimplePackage.json";

                //对应的包路径
                string packagesDirectory = _packagesPath + "/"+ directoryName;
                //包文件夹的Meta文件
                string packagesDirectoryMeta = _packagesPath + "/"+ directoryName + ".meta";
                
                //对应的插件路径
                string pluginsDirectory = _pluginsPath + "/" + directoryName;
                //插件文件夹的Meta文件
                string pluginsDirectoryMeta = _pluginsPath + "/"+ directoryName + ".meta";
                
                RefreshJsonFile(index, simplePackageJson);

                if (pj[index].active)
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
                                CopyFileAndDir(VARIABLE, _pluginsPath + "/" +  VARIABLE.Split("\\").Last());
                            }

                            //获取Patch文件夹内(包括子文件夹)的所有文件的路径
                            string[] files = Directory.GetFiles(pluginsDirectory + "/Patch", "*", SearchOption.AllDirectories);
                            for (int i = 0; i < files.Length; i++)
                            {
                                files[i] = files[i].Replace("\\", "/").Replace(pluginsDirectory + "/Patch", _pluginsPath);
                                // Debug.Log(files[i]);
                            }
                            pj[index].patchFilePaths = files;
                            
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

                        if (pj[index].patchFilePaths.Length != 0)
                        {
                            foreach (var VARIABLE in pj[index].patchFilePaths)
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

            // Language.Instance.languageSet = languageSettings;
            // EditorUtility.SetDirty(Language.Instance);
            // Language.Instance.OnValidate();
            
            AssetDatabase.Refresh();
        }

        #endregion


        
        #region 重要函数
        
        private void RefreshJsonFile(int index, string simplePackageJson)
        {
            //PJ序列化为Json
            string PJstring = JsonUtility.ToJson(pj[index]).Replace("\",","\",\n");
            //更新包根目录下的SimplePackage.json文件
            File.WriteAllText(simplePackageJson, PJstring);
            //重新反序列化为PJ
            PJstring = File.ReadAllText(simplePackageJson);
            pj[index] = JsonUtility.FromJson<PackageJson>(PJstring);
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
        
        #endregion

        
    }
}

#endif