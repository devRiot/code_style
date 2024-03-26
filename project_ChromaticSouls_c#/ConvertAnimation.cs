using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Collections.Generic;

public class ConvertAnimation : EditorWindow 
{
    protected static ConvertAnimation window;    
    protected static string[] animationPaths = new[]
    {
        "Assets/Art/Characters/",
        "Assets/Art/Monsters/_size0_512_256",
        "Assets/Art/Monsters/_size0_512_512",
        "Assets/Art/Monsters/_size1_1024_512",
        "Assets/Art/Monsters/_size2_1024_1024",
        "Assets/Art/Monsters_season2/_size0_512_256",
        "Assets/Art/Monsters_season2/_size0_512_512",
        "Assets/Art/Monsters_season2/_size1_1024_512",
        "Assets/Art/Monsters_season2/_size2_1024_1024",
    };    
    static System.Reflection.MethodInfo miSetInterpolation;
    static System.Reflection.MethodInfo miRemapAnimationBindingForRotationCurves;
    static Type rotationCurveInterpolationModeType;
    static object rawEulerValue;

    [MenuItem("DevToolsS2/Convert/[Tool] Convert Animation")]
    static void Init()
    {
        window = (ConvertAnimation)EditorWindow.GetWindow(typeof(ConvertAnimation));       

        window.Show();
    }

    void OnEnable()
    {
        window = this;        
    }

    void OnDisable()
    {
        window = null;
    }       

    void OnGUI()
    {
        for (int i = 0; i < animationPaths.Length; i++)
        {
            int index = i + 1;

            EditorGUILayout.BeginHorizontal();        

            animationPaths[i] = EditorGUILayout.TextField("Animation Path" + index, animationPaths[i]);        
        
            if (GUILayout.Button("Convert", GUILayout.Width(60)) == true)
            {
                ConvertAnimationClips(animationPaths[i]);
            }

            EditorGUILayout.EndHorizontal();        
        }
    }

    static public void ShowProgress(float val)
    {
        EditorUtility.DisplayProgressBar("Converting", "Converting animations...", val);
    }

    void ConvertAnimationClips(string path)
    {
        ShowProgress(0f);
        InitRotationCurveInterpolation();

        string[] paths = new string[1];

        paths[0] = path;

        string[] assetGUIDs = AssetDatabase.FindAssets("t:AnimationClip", paths);
        string message = "Are you sure you want to covert these animations?\n\n";

        for (int i = 0; i < assetGUIDs.Length; i++)
        {
            string clipPath = AssetDatabase.GUIDToAssetPath(assetGUIDs[i]);

            message += clipPath;
            message += "\n";
        }

        if (EditorUtility.DisplayDialog(
            "Convert Animation",
            message,
            "Ok",
            "Cancel") == true)
        {
            int count = assetGUIDs.Length;
            for (int i = 0; i < assetGUIDs.Length; i++)
            {
                string clipPath = AssetDatabase.GUIDToAssetPath(assetGUIDs[i]);
                AnimationClip animClip = AssetDatabase.LoadAssetAtPath(clipPath, typeof(AnimationClip)) as AnimationClip;

                ShowProgress((float)(i+1) / count);
                ConvertAnimationClip(clipPath, animClip);
            }
        }

        EditorUtility.ClearProgressBar();
        EditorUtility.DisplayDialog("Completed", "Converting progress is completed.\n" + "(" + path +")", "OK");
        Debug.Log("[Completed] Animation clips converted! (" + path + ")");
    }    

    public static void InitRotationCurveInterpolation()
    {
        Type classType = FindScriptClassType("RotationCurveInterpolation");

        miSetInterpolation = classType.GetMethod("SetInterpolation", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        miRemapAnimationBindingForRotationCurves = classType.GetMethod("RemapAnimationBindingForRotationCurves", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);            

        Type enumType = GetScriptClassType("UnityEditor.RotationCurveInterpolation+Mode");

        System.Reflection.FieldInfo fi = enumType.GetField("RawEuler");        

        rawEulerValue = fi.GetValue(null);        
    }

    void ConvertAnimationClip(string clipPath, AnimationClip animClip)
    {        
        EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(animClip);        

        for (int i = 0; i < curveBindings.Length; i++)
        {
            curveBindings[i] = RemapAnimationBindingForRotationCurves(curveBindings[i], animClip);
        }

        SetInterpolation(animClip, curveBindings);        
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    public static EditorCurveBinding RemapAnimationBindingForRotationCurves(EditorCurveBinding curveBinding, AnimationClip clip)
    {
        return (EditorCurveBinding)miRemapAnimationBindingForRotationCurves.Invoke(null, new object[] { curveBinding, clip });
    }

    public static void SetInterpolation(AnimationClip clip, EditorCurveBinding[] curveBindings)
    {
        miSetInterpolation.Invoke(null, new object[] { clip, curveBindings, rawEulerValue });            
    }

    public static Type FindScriptClassType(string className)
    {
        System.Reflection.Assembly[] referencedAssemblies = System.AppDomain.CurrentDomain.GetAssemblies();

        foreach (System.Reflection.Assembly referencedAssembly in referencedAssemblies)
        {
            Type[] types = referencedAssembly.GetTypes();

            foreach (Type type in types)
            {
                if (string.Equals(type.Name, className))
                {
                    return type;
                }
            }
        }

        return null;
    }

    public static Type GetScriptClassType(string className)
    {
        Type result = null;
        System.Reflection.Assembly[] referencedAssemblies = System.AppDomain.CurrentDomain.GetAssemblies();

        foreach (System.Reflection.Assembly referencedAssembly in referencedAssemblies)
        {
            result = referencedAssembly.GetType(className);

            if (result != null)
            {
                break;
            }
        }

        return result;
    }    

    bool RemoveCurveKeys(AnimationCurve curve, float error)
    {
        bool result = true;        
        float value = Mathf.Abs(curve[0].value);

        for (int j = 1; j < curve.length; j++)
        {
            if (Mathf.Abs(Mathf.Abs(curve.keys[j].value) - value) > error)
            {
                result = false;

                break;
            }
        }

        if (result == true)
        {
            for (int j = curve.length - 2; j >= 1; j--)
            {
                curve.RemoveKey(j);
            }            
        }        

        return result;
    }

    string GetFilePathWithoutExtension(string path)
    {
        string result = path;
        int lastIndex = result.LastIndexOf('.');

        if (lastIndex == -1)
        {
            Debug.Log("NHSmoothMovesConverter.GetFilePathWithoutExtension() Failed - invalid (path)" + path);

            return null;
        }

        result = result.Remove(lastIndex);

        return result;
    }

    void ConvertAnimationClip(string clipPath, AnimationClip animClip, float positionErrorValue, float rotationErrorValue)
    {
        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(animClip);
        string convertedClipPath = GetFilePathWithoutExtension(clipPath) + "_converted.anim";
          
        AnimationClip newClip = new AnimationClip();
        int deleteCount = 0;
        int positionDeleteCount = 0;
        int rotationDeleteCount = 0;

        AnimationUtility.SetAnimationClipSettings(newClip, settings);

        // ----------------
        EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(animClip);

        for (int i = 0; i < curveBindings.Length; i++)
        {
            EditorCurveBinding curveBinding = curveBindings[i];
            AnimationCurve curve = AnimationUtility.GetEditorCurve(animClip, curveBinding);

            if (curveBinding.propertyName.Contains("Position") == true && curve.length >= 3)
            {
                if (RemoveCurveKeys(curve, positionErrorValue) == true)
                {
                    positionDeleteCount++;
                    deleteCount++;
                }
            }
            else if (curveBinding.propertyName.Contains("Rotation") == true && curve.length >= 3)
            {
                if (RemoveCurveKeys(curve, rotationErrorValue) == true)
                {
                    rotationDeleteCount++;
                    deleteCount++;
                }
            }

            AnimationUtility.SetEditorCurve(newClip, curveBinding, curve);
        }

        Debug.Log("(ac.Length)" + curveBindings.Length +
            "(delCount)" + deleteCount +
            "(posDelCount)" + positionDeleteCount +
            "(rotDelCount)" + rotationDeleteCount +
            "(posError)" + positionErrorValue +
            "(rotError)" + rotationErrorValue);
        // ----------------

        AssetDatabase.CreateAsset(newClip, convertedClipPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
