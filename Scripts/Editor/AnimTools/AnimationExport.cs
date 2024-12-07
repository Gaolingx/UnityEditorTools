using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AnimationExport
{
    private float index;

    /// <summary>
    /// name为动作名称
    /// space为每个动作之间的间隔,秒为单位
    /// </summary>
    /// <param name="animator"></param>
    /// <param name="name"></param>
    /// <param name="space"></param>
    public void Export(Animator animator,string name,float space,int frameRate,string path)
    {
        var animationPath = "Assets/" + name + ".anim";

        if (AssetDatabase.IsValidFolder(path))
        {
            animationPath = path;
        }

        var output_clip = new AnimationClip()
        {
            name = name,
            legacy = false,
            wrapMode = WrapMode.Once,
            frameRate = frameRate
        };

        index = 0;

        RuntimeAnimatorController ac = animator.runtimeAnimatorController;
        AnimationClip[] ori_clips = ac.animationClips;
        if (null == ori_clips || ori_clips.Length <= 0) return;
        AnimationClip current_clip;
        for (int i = 0, length = ori_clips.Length; i < length; i++)
        {
            current_clip = ac.animationClips[i];
            if (current_clip != null)
            {
                float keys_lenght = AnimCopy(current_clip, ref output_clip, index);
                index += keys_lenght + space;
            }
        }

        AssetDatabase.CreateAsset(output_clip, animationPath); //AssetDatabase中的路径都是相对Asset的  如果指定路径已存在asset则会被删除，然后创建新的asset

        AssetDatabase.SaveAssets();//保存修改

        AssetDatabase.Refresh();
    }

    /// <summary>
    /// name为动作名称
    /// space为每个动作之间的间隔,秒为单位
    /// </summary>
    /// <param name="animator"></param>
    /// <param name="name"></param>
    /// <param name="space"></param>
    public void Export(GameObject obj, List<AnimationClip> clips, string name)
    {        
        AnimationClip current_clip;
        for (int i = 0, length = clips.Count; i < length; i++)
        {
            current_clip = clips[i];
            if (current_clip != null)
            {
                current_clip.legacy = true;

                var _animation = obj.GetComponent<Animation>();
                if (_animation != null)
                {
                    GameObject.DestroyImmediate(_animation);
                }
                
                _animation = obj.AddComponent<Animation>();

                var _name = current_clip.name;
                var animationPath = "Assets/" + _name + ".fbx";
                _animation.AddClip(current_clip, _name);
                _animation.clip = current_clip;
                UnityEditor.Formats.Fbx.Exporter.ModelExporter.ExportObject(animationPath, _animation);
            }
        }


    }

    float AnimCopy(AnimationClip srcClip, ref AnimationClip outputClip, float start_index)
    {
        //AnimationClipSettings setting = AnimationUtility.GetAnimationClipSettings(srcClip);//获取AnimationClipSettings

        //AnimationUtility.SetAnimationClipSettings(newClip, setting);//设置新clip的AnimationClipSettings

        //newClip.frameRate = srcClip.frameRate;//设置新clip的帧率

        float keys_count_max = 0;

        EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(srcClip);//获取clip的curveBinds

        keys_count_max = srcClip.length;

        for (int i = 0; i < curveBindings.Length; i++)
        {
            AnimationCurve ori_curve = AnimationUtility.GetEditorCurve(srcClip, curveBindings[i]);
                      
            AnimationCurve output_curve = AnimationUtility.GetEditorCurve(outputClip, curveBindings[i]);

            AnimationCurve new_curve = new AnimationCurve();

            if (output_curve != null && output_curve.length > 0)
            {
                for (int j = 0; j < output_curve.keys.Length; j++)
                {
                    new_curve.AddKey(output_curve.keys[j].time, output_curve.keys[j].value);
                }
            }

            if (ori_curve != null && ori_curve.length > 0)
            {
                for (int j = 0; j < ori_curve.keys.Length; j++)
                {
                    new_curve.AddKey(start_index + ori_curve.keys[j].time, ori_curve.keys[j].value);
                }
            }

            AnimationUtility.SetEditorCurve(outputClip, curveBindings[i], new_curve);
        }

        return keys_count_max;
    }

}
