using UnityEngine;
using Beebyte.Obfuscator;

public class ObfuscatorExample : MonoBehaviour
{
    public Color publicObfuscatedField = Color.red; // <- Obfuscated
    private Color privateObfuscatedField = Color.blue; // <- Obfuscated

    private string normalLiteral = "Typical string";

    //Enable string obfuscation for the following:
    private string hiddenLiteral = "^Cannot see me^"; // The client will resolve this string as "Cannot see me", but a code inspector will show it as a byte array containing jibberish

    [Rename("RenamedPublicField")]
    public Color OldPublicField = Color.black; // <- Renamed to RenamedPublicField

    [Skip]
    private Color visibleColor = Color.blue; // <- This is left as visibleColor
    [SkipRename]
    private Color anotherVisibleColor = Color.blue; // <- This is left as anotherVisibleColor

    void Start() // <- Key MonoBehaviour methods like Start are left untouched
    {
        Debug.Log("Started Example");

        Debug.Log(normalLiteral);
        Debug.Log(hiddenLiteral);
        Debug.Log(visibleColor);
        Debug.Log(anotherVisibleColor);
    }

    public void ObfuscatedMethod(Color c) // <- Obfuscated method name and parameter
    {
        this.privateObfuscatedField = c;
    }

    public Color ObfuscatedMethod() // <- Obfuscated
    {
        return this.privateObfuscatedField;
    }

    [SkipRename]
    public Color SkipRenameMethod(Color obfuscatedParameter) // <- Method name is left as SkipRenameMethod, parameter is obfuscated
    {
        return obfuscatedParameter;
    }

    [System.Reflection.Obfuscation(ApplyToMembers=false)] // This is equivalent to [SkipRename]
    public Color EquivalentMethod(Color obfuscatedParameter) // <- Method name is left as EquivalentMethod, parameter is obfuscated
    {
        return obfuscatedParameter;
    }

    [Skip]
    public Color SkipMethod(Color visibleParameter) // <- Nothing in this method gets obfuscated, including the parameter
    {
        return visibleParameter;
    }

    [Rename("MyVisibleRename")]
    public Color OldName(Color obfuscatedParameter) // <- Method name is changed to MyVisibleRename, parameter is obfuscated
    {
        return obfuscatedParameter;
    }

    [SkipRename]
    public void OnButtonClick() // <- Button clicks assigned through the inspector should ALWAYS use the SkipRename flag, otherwise they will silently fail
    {
        /*
         * Enable string obfuscation for the following.
         * It worth noting here that obfuscated string literals are best declared as class static variables for performance reasons,
         * however the following examples will still work.
         */
        Debug.Log("^Button was clicked1^"); // <- This gets obfuscated, and will print: Button was clicked1

        Debug.Log("Button " + "^was^" + "clicked2"); // This won't work and will print: Button ^was^ clicked2

        string was = "^was^";
        Debug.Log("Button " + was + "clicked3"); // This works, and prints: Button was clicked3

        Debug.Log("Button was clicked4");
    }

    [SkipRename]
    public void OnAnimationEvent() // Animation events assigned through the inspector should be excluded from obfuscation
    {
    }    

    public void ObfuscatedButtonMethod() // Button click methods can be obfuscated if they are assigned programatically, e.g. button.onClick.AddListener(ObfuscatedButtonMethod);
    {
    }

    [ObfuscateLiterals] //New in version 1.17.0
    private void LiterallyLotsOfLiterals()
    {
        string we = "We";
        Debug.Log("Here " + we + "have three obfuscated literals. No markers needed!");
    }

    private System.Collections.IEnumerator MyAmazingMethod() // <-- With default options Coroutine methods are obfuscated too!
    {
        //...
        yield return null;
    }
    
    private void SomeMethodCallingACoroutine()
    {
        StartCoroutine("MyAmazingMethod"); // <-- With default options "MyAmazingMethod" here will be automatically
                                                      // substituted for the new obfuscated name assigned to MyAmazingMethod()
    }


#if UNITY_2018_2_OR_NEWER
#else
#pragma warning disable 618
    [RPC]
    [ReplaceLiteralsWithName]
    [Rename("9xy")]
    void MyRPCMethod(string message) //This method is renamed to 9xy and all references of the exact string "MyRPCMethod" are replaced with "9xy"
    {
    }

    void NetworkingCode()
    {
        NetworkView nView = null;

        //Because MyRPCMethod was annotated with the [ReplaceLiteralsWithName] attribute, the "MyRPCMethod" string here will be replaced with "9xy"
        nView.RPC("MyRPCMethod", RPCMode.AllBuffered, "Hello World");

        UnityEngine.Debug.Log("Today I took my MyRPCMethod for a walk"); //This string is not changed.
    }

    [RPC]
    void AnotherRPCMethod() //by default, RPC annotated methods are not renamed
    {
    }

    /*
     * For 3rd party RPC annotations to be recognised, add the attribute's canonical classname to the "Alternate RPC Annotations" array in Options
     * e.g. Third.Party.SomeOtherRPC
     *
    [SomeOtherRPC]
    void NonUnityRPCMethod()
    {
    }
    */
#pragma warning restore 618
#endif
}

namespace MyNamespace
{
    [Rename("MovedNamespace.ObfuscatedName")] //Rename can change the namespace too!
    public class MyClass
    {
    }

    [Rename("NewName")] //When the namespace isn't specified on a class, it will either leave it in the same namespace or move it to the default namespace if Strip Namespaces is enabled
    public class MyOtherClass
    {
    }
}
