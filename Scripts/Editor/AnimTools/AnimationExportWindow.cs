using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AnimationExportWindow: EditorWindow
{
    public static GameObject targetGo;

    private static AnimationExport baker;

    private static string path = "Asset/";

    private static string animator_name = "Default";

    private static float space = 0f;

    private static int frameRate = 30;

    [SerializeField]//必须要加  
    protected List<UnityEngine.AnimationClip> _animationClips = new List<UnityEngine.AnimationClip>();
    //序列化对象  
    protected SerializedObject _serializedObject;
    //序列化属性  
    protected SerializedProperty _assetLstProperty;

    GUIStyle fontSytle1;

    private void OnEnable()
    {
        fontSytle1 = new GUIStyle();
        fontSytle1.fontSize = 15;
        fontSytle1.normal.textColor = Color.yellow;
        fontSytle1.fontStyle = FontStyle.Bold;
        fontSytle1.alignment = TextAnchor.MiddleCenter;
        fontSytle1.wordWrap = true;

        //使用当前类初始化  
        _serializedObject = new SerializedObject(this);
        //获取当前类中可序列话的属性  
        _assetLstProperty = _serializedObject.FindProperty("_animationClips");
    }

    [MenuItem("Art Tools/AnimationExport")]
    public static void ShowWindow()
    {
        var win = EditorWindow.GetWindow(typeof(AnimationExportWindow));
        baker = new AnimationExport();
        win.Show();
    }

    void OnGUI()
    {
        GUILayout.BeginVertical("box");
        GUILayout.Space(10f);
        GUILayout.Label("-批量合批动画工具-", fontSytle1);
        GUILayout.Space(10f);

        targetGo = (GameObject)EditorGUILayout.ObjectField("目标动画机(Animator):",targetGo, typeof(GameObject), true);

        animator_name = EditorGUILayout.TextField("输出动作名称(name):", animator_name);

        path = EditorGUILayout.TextField("输出路径(path):", path);

        space = EditorGUILayout.FloatField("动作间隔(space/s):", space);

        frameRate = EditorGUILayout.IntField("帧率(frameRate):", frameRate);

        var animationPath = "Assets/" + animator_name + ".anim";

        if (!string.IsNullOrEmpty(path) &&
            !string.IsNullOrEmpty(System.IO.Path.GetDirectoryName(path)) &&
            System.IO.Directory.Exists(path)
            )
        {
            animationPath = path;
        }

       GUILayout.Label("输出路径(output_path):" + animationPath);

        if (GUILayout.Button("Generate"))
        {
            if (targetGo == null)
            {
                EditorUtility.DisplayDialog("err", "目标动画机 is null！", "OK");
                return;
            }

            if (baker == null)
            {
                baker = new AnimationExport();
            }

            var animator = targetGo.GetComponent<Animator>();

            if (animator == null)
            {
                EditorUtility.DisplayDialog("err", "Animator is null！", "OK");
                return;
            }

            baker.Export(animator, animator_name, space, frameRate, animationPath);
        }
        GUILayout.EndVertical();

        GUILayout.BeginVertical("box");
        GUILayout.Space(10f);
        GUILayout.Label("-批量生成Fbx工具-", fontSytle1);
        GUILayout.Space(10f);

        targetGo = (GameObject)EditorGUILayout.ObjectField("目标物体", targetGo, typeof(GameObject), true);

        if (GUILayout.Button("Generate"))
        {
            if (targetGo == null)
            {
                EditorUtility.DisplayDialog("err", "目标物体为空！", "OK");
                return;
            }

            if (baker == null)
            {
                baker = new AnimationExport();
            }

            if (_animationClips.Count <= 0)
            {
                EditorUtility.DisplayDialog("err", "AnimationClips is null！", "OK");
                return;
            }

            baker.Export(targetGo,_animationClips, animator_name);
        }
        
        //更新  
        _serializedObject.Update();

        //开始检查是否有修改  
        EditorGUI.BeginChangeCheck();

        //显示属性  
        //第二个参数必须为true，否则无法显示子节点即List内容  
        EditorGUILayout.PropertyField(_assetLstProperty, true);

        //结束检查是否有修改  
        if (EditorGUI.EndChangeCheck())
        {//提交修改  
            _serializedObject.ApplyModifiedProperties();
        }

        GUILayout.EndVertical();
    }
}
