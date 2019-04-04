using HolisticWare.Xamarin.Tools.Bindings.XamarinAndroid.AndroidX.Migraineator;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace Xamarin.AndroidX.Migration.Tests.Cecilfier
{
    public class JniMigrationTests : BaseTests
    {
        [Theory]
        [InlineData("", "")]
        [InlineData("()V", "()V")]
        [InlineData
            (
                "(Landroid/content/Context;)Landroid/support/v4/app/Fragment;",
                "(Landroid/content/Context;)Landroidx/fragment/app/Fragment;"
            )
        ]
        [InlineData
            (
                "CreateFragment.(Landroid/content/Context;)Landroid/support/v4/app/Fragment;",
                "CreateFragment.(Landroid/content/Context;)Landroidx/fragment/app/Fragment;"
            )
        ]
        [InlineData
            (
                "(Landroid/content/Context;)Lcom/xamarin/aarxercise/SimpleFragment;",
                "(Landroid/content/Context;)Lcom/xamarin/aarxercise/SimpleFragment;"
            )
        ]
        [InlineData
            (
                "CreateSimpleFragment.(Landroid/content/Context;)Lcom/xamarin/aarxercise/SimpleFragment;",
                "CreateSimpleFragment.(Landroid/content/Context;)Lcom/xamarin/aarxercise/SimpleFragment;"
            )
        ]
        [InlineData
            (
                "(Landroid/support/v4/app/Fragment;Ljava/lang/String;)V",
                "(Landroidx/fragment/app/Fragment;Ljava/lang/String;)V"
            )
        ]
        [InlineData
            (
                "UpdateFragment.(Landroid/support/v4/app/Fragment;Ljava/lang/String;)V",
                "UpdateFragment.(Landroidx/fragment/app/Fragment;Ljava/lang/String;)V"
            )
        ]
        [InlineData
            (
                "(Lcom/xamarin/aarxercise/SimpleFragment;Ljava/lang/String;)V",
                "(Lcom/xamarin/aarxercise/SimpleFragment;Ljava/lang/String;)V"
            )
        ]
        [InlineData
            (
                "UpdateSimpleFragment.(Lcom/xamarin/aarxercise/SimpleFragment;Ljava/lang/String;)V",
                "UpdateSimpleFragment.(Lcom/xamarin/aarxercise/SimpleFragment;Ljava/lang/String;)V"
            )
        ]
        [InlineData
            (
                //mc++ "([Landroid/support/v4/graphics/PathParser;[Landroid/support/v4/graphics/PathParser;)V",
                //mc++ "([Landroidx/core/graphics/PathParser;[Landroidx/core/graphics/PathParser;)V"
                "(Landroid/support/v4/graphics/PathParser;Landroid/support/v4/graphics/PathParser;)V",
                "(Landroidx/core/graphics/PathParser;Landroidx/core/graphics/PathParser;)V"
            )
        ]
        [InlineData
            (
                //mc++ "([Landroid/support/v4/graphics/PathParser$PathDataNode;[Landroid/support/v4/graphics/PathParser$PathDataNode;)V",
                //mc++ "([Landroidx/core/graphics/PathParser$PathDataNode;[Landroidx/core/graphics/PathParser$PathDataNode;)V"
                "(Landroid/support/v4/graphics/PathParser$PathDataNode;Landroid/support/v4/graphics/PathParser$PathDataNode;)V",
                "(Landroidx/core/graphics/PathParser$PathDataNode;Landroidx/core/graphics/PathParser$PathDataNode;)V"
            )
            ]
        [InlineData
            (
                //mc++ "([Ljava/lang/Object;[[Ljava/lang/Object;)[Ljava/lang/Object;",
                //mc++ "([Ljava/lang/Object;[[Ljava/lang/Object;)[Ljava/lang/Object;"
                "(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;",
                "(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;"
            )
        ]
        [InlineData
            (
                //mc++ "([Ljava/lang/Object;[[Landroid/support/v4/graphics/PathParser;)[Ljava/lang/Object;",
                //mc++ "([Ljava/lang/Object;[[Landroidx/core/graphics/PathParser;)[Ljava/lang/Object;"
                "(Ljava/lang/Object;Landroid/support/v4/graphics/PathParser;)Ljava/lang/Object;",
                "(Ljava/lang/Object;Landroidx/core/graphics/PathParser;)Ljava/lang/Object;"
            )
        ]
        [InlineData
            (
                "java/lang/Object",
                "java/lang/Object"
            )
        ]
        [InlineData
            (
                "android/support/v4/app/Fragment",
                "androidx/fragment/app/Fragment"
            )
        ]
        [InlineData
            (
                "android/support/v7/app/ActionBar$Tab",
                "androidx/appcompat/app/ActionBar$Tab"
            )
        ]
        [InlineData
            (
                "android/support/v7/app/ActionBarDrawerToggle$Delegate",
                "androidx/appcompat/app/ActionBarDrawerToggle$Delegate"
            )
        ]
        [InlineData
            (
                "android/support/v7/app/ActionBar$Tab$ThisDoesNotExist",
                "androidx/appcompat/app/ActionBar$Tab$ThisDoesNotExist"
            )
        ]
        [InlineData
            (
                "android/support/v7/app/ActionBarDrawerToggle$ThisDoesNotExist",
                "androidx/appcompat/app/ActionBarDrawerToggle$ThisDoesNotExist"
            )
        ]
        [InlineData
            (
                "android/support/v7/app/ActionBarDrawerToggle$ThisDoesNotExist$AndNeitherDoesThis",
                "androidx/appcompat/app/ActionBarDrawerToggle$ThisDoesNotExist$AndNeitherDoesThis"
            )
        ]
        [InlineData
            (
                "(I)Landroid/support/v7/app/AlertDialog$Builder;",
                "(I)Landroidx/appcompat/app/AlertDialog$Builder;"
            )
        ]
        [InlineData
            (
                "(L;L;L;)Landroid/support/v7/app/AlertDialog$Builder;",
                "(L;L;L;)Landroidx/appcompat/app/AlertDialog$Builder;"
            )
        ]
        //----------------------------------------------------------------------------------
        // some tests from useless analysis
        [InlineData
            (
                "(Ljava/lang/Object;Ljava/lang/Object;)Landroid/arch/core/internal/SafeIterableMap$Entry;",
                "(Ljava/lang/Object;Ljava/lang/Object;)Landroidx/arch/core/internal/SafeIterableMap$Entry;"
            )
        ]
	    [InlineData
            (
			    "(Ljava/lang/String;)Landroid/support/annotation/RestrictTo$Scope;",
			    "(Ljava/lang/String;)Landroidx/annotation/RestrictTo$Scope;"
            )
        ]
	    [InlineData
            (
			    "(Landroid/arch/lifecycle/LifecycleOwner;Landroid/arch/lifecycle/Lifecycle$Event;ZLandroid/arch/lifecycle/MethodCallsLogger;)V",
			    "(Landroid/arch/lifecycle/ifecycleOwner;Landroid/arch/lifecycle/ifecycle$Event;ZLandroid/arch/lifecycle/MethodCallsLogger;)V"
            )
        ]
	    [InlineData
            (
			    "(Landroid/support/v4/app/Fragment;)Landroid/arch/lifecycle/HolderFragment;",
			    "(Landroidx/fragment/app/Fragment;)Landroid/arch/lifecycle/HolderFragment;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/v4/app/Fragment;)Landroid/arch/lifecycle/HolderFragment;",
			    "(Landroidx/fragment/app/Fragment;)Landroid/arch/lifecycle/HolderFragment;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/v4/app/FragmentActivity;)Landroid/arch/lifecycle/HolderFragment;",
			    "(Landroidx/fragment/app/FragmentActivity;)Landroid/arch/lifecycle/HolderFragment;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/v4/app/FragmentActivity;)Landroid/arch/lifecycle/HolderFragment;",
			    "(Landroidx/fragment/app/FragmentActivity;)Landroid/arch/lifecycle/HolderFragment;"
            )
        ]
	    [InlineData
            (
			    "(Landroid/support/v4/app/Fragment;)Landroid/arch/lifecycle/ViewModelProvider;",
			    "(Landroidx/fragment/app/Fragment;)Landroidx/lifecycle/ViewModelProvider;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/v4/app/Fragment;Landroid/arch/lifecycle/ViewModelProvider$Factory;)Landroid/arch/lifecycle/ViewModelProvider;",
			    "(Landroidx/fragment/app/Fragment;Landroidx/lifecycle/ViewModelProvider$Factory;)Landroidx/lifecycle/ViewModelProvider;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/v4/app/Fragment;Landroid/arch/lifecycle/ViewModelProvider$Factory;)Landroid/arch/lifecycle/ViewModelProvider;",
			    "(Landroidx/fragment/app/Fragment;Landroidx/lifecycle/ViewModelProvider$Factory;)Landroidx/lifecycle/ViewModelProvider;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/v4/app/FragmentActivity;)Landroid/arch/lifecycle/ViewModelProvider;",
			    "(Landroidx/fragment/app/FragmentActivity;)Landroidx/lifecycle/ViewModelProvider;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/v4/app/FragmentActivity;)Landroid/arch/lifecycle/ViewModelProvider;",
			    "(Landroidx/fragment/app/FragmentActivity;)Landroidx/lifecycle/ViewModelProvider;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/v4/app/FragmentActivity;Landroid/arch/lifecycle/ViewModelProvider$Factory;)Landroid/arch/lifecycle/ViewModelProvider;",
			    "(Landroidx/fragment/app/FragmentActivity;Landroidx/lifecycle/ViewModelProvider$Factory;)Landroidx/lifecycle/ViewModelProvider;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/v4/app/FragmentActivity;Landroid/arch/lifecycle/ViewModelProvider$Factory;)Landroid/arch/lifecycle/ViewModelProvider;",
			    "(Landroidx/fragment/app/FragmentActivity;Landroidx/lifecycle/ViewModelProvider$Factory;)Landroidx/lifecycle/ViewModelProvider;"
            )
        ]
                        

	    [InlineData
            (
			    "android/arch/lifecycle/ViewModelStores",
			    "androidx/lifecycle/ViewModelStores"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/v4/app/Fragment;)Landroid/arch/lifecycle/ViewModelStore;",
			    "(Landroidx/fragment/app/Fragment;)Landroidx/lifecycle/ViewModelStore;"
            )
        ]
	    [InlineData
            (
			    "(Landroidx/versionedparcelable/VersionedParcel;)Landroid/support/v4/graphics/drawable/IconCompat;",
			    "(Landroidx/versionedparcelable/VersionedParcel;)Landroidx/core/graphics/drawable/IconCompat;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroidx/versionedparcelable/VersionedParcel;)Landroid/support/v4/graphics/drawable/IconCompat;",
			    "(Landroidx/versionedparcelable/VersionedParcel;)Landroidx/core/graphics/drawable/IconCompat;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/v4/graphics/drawable/IconCompat;Landroidx/versionedparcelable/VersionedParcel;)V",
			    "(Landroidx/core/graphics/drawable/IconCompat;Landroidx/versionedparcelable/VersionedParcel;)V"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/v4/graphics/drawable/IconCompat;Landroidx/versionedparcelable/VersionedParcel;)V",
			    "(Landroidx/core/graphics/drawable/IconCompat;Landroidx/versionedparcelable/VersionedParcel;)V"
            )
        ]
	    [InlineData
            (
			    "(Landroid/view/inputmethod/InputConnection;Landroid/view/inputmethod/EditorInfo;Landroid/support/v13/view/inputmethod/InputConnectionCompat$OnCommitContentListener;)Landroid/view/inputmethod/InputConnection;",
			    "(Landroid/view/inputmethod/InputConnection;Landroid/view/inputmethod/EditorInfo;Landroidx/core/view/inputmethod/InputConnectionCompat$OnCommitContentistener;)Landroid/view/inputmethod/InputConnection;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/view/inputmethod/InputConnection;Landroid/view/inputmethod/EditorInfo;Landroid/support/v13/view/inputmethod/InputConnectionCompat$OnCommitContentListener;)Landroid/view/inputmethod/InputConnection;",
			    "(Landroid/view/inputmethod/InputConnection;Landroid/view/inputmethod/EditorInfo;Landroidx/core/view/inputmethod/InputConnectionCompat$OnCommitContentistener;)Landroid/view/inputmethod/InputConnection;"
            )
        ]
	    [InlineData
            (
			    "(Landroid/view/MenuItem;Landroid/support/v4/view/MenuItemCompat$OnActionExpandListener;)Landroid/view/MenuItem;",
			    "(Landroid/view/MenuItem;Landroidx/core/view/MenuItemCompat$OnActionExpandistener;)Landroid/view/MenuItem;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/view/MenuItem;Landroid/support/v4/view/MenuItemCompat$OnActionExpandListener;)Landroid/view/MenuItem;",
			    "(Landroid/view/MenuItem;Landroidx/core/view/MenuItemCompat$OnActionExpandistener;)Landroid/view/MenuItem;"
            )
        ]
	    [InlineData
            (
			    "(Landroid/view/View;Landroid/support/v4/view/WindowInsetsCompat;)Landroid/support/v4/view/WindowInsetsCompat;",
			    "(Landroid/view/View;Landroidx/core/view/WindowInsetsCompat;)Landroidx/core/view/WindowInsetsCompat;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/view/View;Landroid/support/v4/view/WindowInsetsCompat;)Landroid/support/v4/view/WindowInsetsCompat;",
			    "(Landroid/view/View;Landroidx/core/view/WindowInsetsCompat;)Landroidx/core/view/WindowInsetsCompat;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/view/View;)Landroid/support/v4/view/accessibility/AccessibilityNodeProviderCompat;",
			    "(Landroid/view/View;)Landroidx/core/view/accessibility/AccessibilityNodeProviderCompat;"
            )
        ]
	    [InlineData
            (
			    "(Landroid/support/v4/view/ViewPropertyAnimatorUpdateListener;)Landroid/support/v4/view/ViewPropertyAnimatorCompat;",
			    "(Landroid/support/v4/view/ViewPropertyAnimatorUpdateistener;)Landroidx/core/view/ViewPropertyAnimatorCompat;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/v4/view/ViewPropertyAnimatorUpdateListener;)Landroid/support/v4/view/ViewPropertyAnimatorCompat;",
			    "(Landroid/support/v4/view/ViewPropertyAnimatorUpdateistener;)Landroidx/core/view/ViewPropertyAnimatorCompat;"
            )
        ]
	    [InlineData
            (
			    "(Landroid/view/accessibility/AccessibilityEvent;Landroid/support/v4/view/accessibility/AccessibilityRecordCompat;)V",
			    "(Landroid/view/accessibility/AccessibilityEvent;Landroidx/core/view/accessibility/AccessibilityRecordCompat;)V"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/view/accessibility/AccessibilityEvent;Landroid/support/v4/view/accessibility/AccessibilityRecordCompat;)V",
			    "(Landroid/view/accessibility/AccessibilityEvent;Landroidx/core/view/accessibility/AccessibilityRecordCompat;)V"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/view/accessibility/AccessibilityEvent;)Landroid/support/v4/view/accessibility/AccessibilityRecordCompat;",
			    "(Landroid/view/accessibility/AccessibilityEvent;)Landroidx/core/view/accessibility/AccessibilityRecordCompat;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/view/accessibility/AccessibilityEvent;)Landroid/support/v4/view/accessibility/AccessibilityRecordCompat;",
			    "(Landroid/view/accessibility/AccessibilityEvent;)Landroidx/core/view/accessibility/AccessibilityRecordCompat;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/view/accessibility/AccessibilityEvent;I)Landroid/support/v4/view/accessibility/AccessibilityRecordCompat;",
			    "(Landroid/view/accessibility/AccessibilityEvent;I)Landroidx/core/view/accessibility/AccessibilityRecordCompat;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/view/accessibility/AccessibilityEvent;I)Landroid/support/v4/view/accessibility/AccessibilityRecordCompat;",
			    "(Landroid/view/accessibility/AccessibilityEvent;I)Landroidx/core/view/accessibility/AccessibilityRecordCompat;"
            )
        ]
	    [InlineData
            (
			    "(Landroid/view/accessibility/AccessibilityManager;Landroid/support/v4/view/accessibility/AccessibilityManagerCompat$TouchExplorationStateChangeListener;)Z",
			    "(Landroid/view/accessibility/AccessibilityManager;Landroidx/core/view/accessibility/AccessibilityManagerCompat$TouchExplorationStateChangeistener;)Z"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/view/accessibility/AccessibilityManager;Landroid/support/v4/view/accessibility/AccessibilityManagerCompat$TouchExplorationStateChangeListener;)Z",
			    "(Landroid/view/accessibility/AccessibilityManager;Landroidx/core/view/accessibility/AccessibilityManagerCompat$TouchExplorationStateChangeistener;)Z"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/view/accessibility/AccessibilityManager;Landroid/support/v4/view/accessibility/AccessibilityManagerCompat$AccessibilityStateChangeListener;)Z",
			    "(Landroid/view/accessibility/AccessibilityManager;Landroidx/core/view/accessibility/AccessibilityManagerCompat$AccessibilityStateChangeistener;)Z"
            )
        ]
	    [InlineData
            (
			    "(Landroid/support/v4/view/accessibility/AccessibilityNodeInfoCompat;)Landroid/support/v4/view/accessibility/AccessibilityNodeInfoCompat;",
			    "(Landroidx/core/view/accessibility/AccessibilityNodeInfoCompat;)Landroidx/core/view/accessibility/AccessibilityNodeInfoCompat;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/v4/view/accessibility/AccessibilityNodeInfoCompat;)Landroid/support/v4/view/accessibility/AccessibilityNodeInfoCompat;",
			    "(Landroidx/core/view/accessibility/AccessibilityNodeInfoCompat;)Landroidx/core/view/accessibility/AccessibilityNodeInfoCompat;"
            )
        ]
	    [InlineData
            (
			    "(Landroid/content/Context;Landroid/support/v4/provider/FontRequest;Landroid/support/v4/content/res/ResourcesCompat$FontCallback;Landroid/os/Handler;ZII)Landroid/graphics/Typeface;",
			    "(Landroid/content/Context;Landroidx/core/provider/FontRequest;Landroidx/core/content/res/ResourcesCompat$FontCallback;Landroid/os/Handler;ZII)Landroid/graphics/Typeface;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/content/pm/PackageManager;Landroid/support/v4/provider/FontRequest;Landroid/content/res/Resources;)Landroid/content/pm/ProviderInfo;",
			    "(Landroid/content/pm/PackageManager;Landroidx/core/provider/FontRequest;Landroid/content/res/Resources;)Landroid/content/pm/ProviderInfo;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/content/pm/PackageManager;Landroid/support/v4/provider/FontRequest;Landroid/content/res/Resources;)Landroid/content/pm/ProviderInfo;",
			    "(Landroid/content/pm/PackageManager;Landroidx/core/provider/FontRequest;Landroid/content/res/Resources;)Landroid/content/pm/ProviderInfo;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/content/Context;Landroid/support/v4/provider/FontRequest;Landroid/support/v4/provider/FontsContractCompat$FontRequestCallback;Landroid/os/Handler;)V",
			    "(Landroid/content/Context;Landroidx/core/provider/FontRequest;Landroidx/core/provider/FontsContractCompat$FontRequestCallback;Landroid/os/Handler;)V"
            )
        ]
	    [InlineData
            (
			    "(Landroid/support/v4/hardware/fingerprint/FingerprintManagerCompat$CryptoObject;ILandroid/support/v4/os/CancellationSignal;Landroid/support/v4/hardware/fingerprint/FingerprintManagerCompat$AuthenticationCallback;Landroid/os/Handler;)V",
			    "(Landroidx/core/hardware/fingerprint/FingerprintManagerCompat$CryptoObject;ILandroid/support/v4/os/CancellationSignal;Landroidx/core/hardware/fingerprint/FingerprintManagerCompat$AuthenticationCallback;Landroid/os/Handler;)V"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/v4/hardware/fingerprint/FingerprintManagerCompat$CryptoObject;ILandroid/support/v4/os/CancellationSignal;Landroid/support/v4/hardware/fingerprint/FingerprintManagerCompat$AuthenticationCallback;Landroid/os/Handler;)V",
			    "(Landroidx/core/hardware/fingerprint/FingerprintManagerCompat$CryptoObject;ILandroid/support/v4/os/CancellationSignal;Landroidx/core/hardware/fingerprint/FingerprintManagerCompat$AuthenticationCallback;Landroid/os/Handler;)V"
            )
        ]
	    [InlineData
            (
			    "(Landroid/content/Context;Landroid/support/v4/content/res/FontResourcesParserCompat$FamilyResourceEntry;Landroid/content/res/Resources;IILandroid/support/v4/content/res/ResourcesCompat$FontCallback;Landroid/os/Handler;Z)Landroid/graphics/Typeface;",
			    "(Landroid/content/Context;Landroidx/core/content/res/FontResourcesParserCompat$FamilyResourceEntry;Landroid/content/res/Resources;IILandroid/support/v4/content/res/ResourcesCompat$FontCallback;Landroid/os/Handler;Z)Landroid/graphics/Typeface;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/content/Context;Landroid/support/v4/content/res/FontResourcesParserCompat$FamilyResourceEntry;Landroid/content/res/Resources;IILandroid/support/v4/content/res/ResourcesCompat$FontCallback;Landroid/os/Handler;Z)Landroid/graphics/Typeface;",
			    "(Landroid/content/Context;Landroidx/core/content/res/FontResourcesParserCompat$FamilyResourceEntry;Landroid/content/res/Resources;IILandroid/support/v4/content/res/ResourcesCompat$FontCallback;Landroid/os/Handler;Z)Landroid/graphics/Typeface;"
            )
        ]
	    [InlineData
            (
			    "(Landroid/content/res/Resources;Ljava/io/InputStream;)Landroid/support/v4/graphics/drawable/RoundedBitmapDrawable;",
			    "(Landroid/content/res/Resources;Ljava/io/InputStream;)Landroidx/core/graphics/drawable/RoundedBitmapDrawable;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/content/res/Resources;Ljava/lang/String;)Landroid/support/v4/graphics/drawable/RoundedBitmapDrawable;",
			    "(Landroid/content/res/Resources;Ljava/lang/String;)Landroidx/core/graphics/drawable/RoundedBitmapDrawable;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/content/res/Resources;Ljava/lang/String;)Landroid/support/v4/graphics/drawable/RoundedBitmapDrawable;",
			    "(Landroid/content/res/Resources;Ljava/lang/String;)Landroidx/core/graphics/drawable/RoundedBitmapDrawable;"
            )
        ]
	    [InlineData
            (
			    "(Landroid/content/ContentResolver;Landroid/net/Uri;[Ljava/lang/String;Ljava/lang/String;[Ljava/lang/String;Ljava/lang/String;Landroid/support/v4/os/CancellationSignal;)Landroid/database/Cursor;",
			    "(Landroid/content/ContentResolver;Landroid/net/Uri;[Ljava/lang/String;Ljava/lang/String;[Ljava/lang/String;Ljava/lang/String;Landroidx/core/os/CancellationSignal;)Landroid/database/Cursor;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/content/ContentResolver;Landroid/net/Uri;[Ljava/lang/String;Ljava/lang/String;[Ljava/lang/String;Ljava/lang/String;Landroid/support/v4/os/CancellationSignal;)Landroid/database/Cursor;",
			    "(Landroid/content/ContentResolver;Landroid/net/Uri;[Ljava/lang/String;Ljava/lang/String;[Ljava/lang/String;Ljava/lang/String;Landroidx/core/os/CancellationSignal;)Landroid/database/Cursor;"
            )
        ]
	    [InlineData
            (
			    "(Lorg/xmlpull/v1/XmlPullParser;Landroid/content/res/Resources;)Landroid/support/v4/content/res/FontResourcesParserCompat$FamilyResourceEntry;",
			    "(Lorg/xmlpull/v1/XmlPullParser;Landroid/content/res/Resources;)Landroidx/core/content/res/FontResourcesParserCompat$FamilyResourceEntry;"
            )
        ]
                        

	    [InlineData
            (
			    "(Lorg/xmlpull/v1/XmlPullParser;Landroid/content/res/Resources;)Landroid/support/v4/content/res/FontResourcesParserCompat$FamilyResourceEntry;",
			    "(Lorg/xmlpull/v1/XmlPullParser;Landroid/content/res/Resources;)Landroidx/core/content/res/FontResourcesParserCompat$FamilyResourceEntry;"
            )
        ]
	    [InlineData
            (
			    "(Ljava/lang/ClassLoader;Ljava/lang/String;Landroid/content/Intent;)Landroid/content/BroadcastReceiver;",
			    "(Ljava/lang/Classoader;Ljava/lang/String;Landroid/content/Intent;)Landroid/content/BroadcastReceiver;"
            )
        ]
                        

	    [InlineData
            (
			    "(Ljava/lang/ClassLoader;Ljava/lang/String;Landroid/content/Intent;)Landroid/content/BroadcastReceiver;",
			    "(Ljava/lang/Classoader;Ljava/lang/String;Landroid/content/Intent;)Landroid/content/BroadcastReceiver;"
            )
        ]
	    [InlineData
            (
			    "(Landroid/app/PendingIntent;Landroid/support/v4/app/RemoteInput;)Landroid/support/v4/app/NotificationCompat$CarExtender$UnreadConversation$Builder;",
			    "(Landroid/app/PendingIntent;Landroidx/core/app/RemoteInput;)Landroidx/core/app/NotificationCompat$CarExtender$UnreadConversation$Builder;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/app/PendingIntent;Landroid/support/v4/app/RemoteInput;)Landroid/support/v4/app/NotificationCompat$CarExtender$UnreadConversation$Builder;",
			    "(Landroid/app/PendingIntent;Landroidx/core/app/RemoteInput;)Landroidx/core/app/NotificationCompat$CarExtender$UnreadConversation$Builder;"
            )
        ]
	    [InlineData
            (
			    "(Ljava/lang/CharSequence;Landroid/support/v4/text/PrecomputedTextCompat$Params;Ljava/util/concurrent/Executor;)Ljava/util/concurrent/Future;",
			    "(Ljava/lang/CharSequence;Landroidx/core/text/PrecomputedTextCompat$Params;Ljava/util/concurrent/Executor;)Ljava/util/concurrent/Future;"
            )
        ]
                        

	    [InlineData
            (
			    "(Ljava/lang/CharSequence;Landroid/support/v4/text/PrecomputedTextCompat$Params;Ljava/util/concurrent/Executor;)Ljava/util/concurrent/Future;",
			    "(Ljava/lang/CharSequence;Landroidx/core/text/PrecomputedTextCompat$Params;Ljava/util/concurrent/Executor;)Ljava/util/concurrent/Future;"
            )
        ]
	    [InlineData
            (
			    "(Landroid/widget/TextView;Ljava/util/regex/Pattern;Ljava/lang/String;Landroid/text/util/Linkify$MatchFilter;Landroid/text/util/Linkify$TransformFilter;)V",
			    "(Landroid/widget/TextView;Ljava/util/regex/Pattern;Ljava/lang/String;Landroid/text/util/inkify$MatchFilter;Landroid/text/util/inkify$TransformFilter;)V"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/widget/TextView;Ljava/util/regex/Pattern;Ljava/lang/String;Landroid/text/util/Linkify$MatchFilter;Landroid/text/util/Linkify$TransformFilter;)V",
			    "(Landroid/widget/TextView;Ljava/util/regex/Pattern;Ljava/lang/String;Landroid/text/util/inkify$MatchFilter;Landroid/text/util/inkify$TransformFilter;)V"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/widget/TextView;Ljava/util/regex/Pattern;Ljava/lang/String;[Ljava/lang/String;Landroid/text/util/Linkify$MatchFilter;Landroid/text/util/Linkify$TransformFilter;)V",
			    "(Landroid/widget/TextView;Ljava/util/regex/Pattern;Ljava/lang/String;[Ljava/lang/String;Landroid/text/util/inkify$MatchFilter;Landroid/text/util/inkify$TransformFilter;)V"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/widget/TextView;Ljava/util/regex/Pattern;Ljava/lang/String;[Ljava/lang/String;Landroid/text/util/Linkify$MatchFilter;Landroid/text/util/Linkify$TransformFilter;)V",
			    "(Landroid/widget/TextView;Ljava/util/regex/Pattern;Ljava/lang/String;[Ljava/lang/String;Landroid/text/util/inkify$MatchFilter;Landroid/text/util/inkify$TransformFilter;)V"
            )
        ]
	    [InlineData
            (
			    "(Landroid/support/design/widget/CoordinatorLayout;Landroid/view/View;Landroid/support/v4/view/WindowInsetsCompat;)Landroid/support/v4/view/WindowInsetsCompat;",
			    "(Landroid/support/design/widget/Coordinatorayout;Landroid/view/View;Landroidx/core/view/WindowInsetsCompat;)Landroidx/core/view/WindowInsetsCompat;"
            )
        ]
 	    [InlineData
            (
			    "(Landroid/view/ViewGroup;Landroid/support/v4/widget/ViewDragHelper$Callback;)Landroid/support/v4/widget/ViewDragHelper;",
			    "(Landroid/view/ViewGroup;Landroidx/customview/widget/ViewDragHelper$Callback;)Landroidx/customview/widget/ViewDragHelper;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/view/ViewGroup;Landroid/support/v4/widget/ViewDragHelper$Callback;)Landroid/support/v4/widget/ViewDragHelper;",
			    "(Landroid/view/ViewGroup;Landroidx/customview/widget/ViewDragHelper$Callback;)Landroidx/customview/widget/ViewDragHelper;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/view/ViewGroup;FLandroid/support/v4/widget/ViewDragHelper$Callback;)Landroid/support/v4/widget/ViewDragHelper;",
			    "(Landroid/view/ViewGroup;FLandroidx/customview/widget/ViewDragHelper$Callback;)Landroidx/customview/widget/ViewDragHelper;"
            )
        ]
	    [InlineData
            (
			    "(Landroid/support/v4/view/ViewPager;Landroid/support/v4/view/PagerAdapter;Landroid/support/v4/view/PagerAdapter;)V",
			    "(Landroidx/viewpager/widget/ViewPager;Landroidx/viewpager/widget/PagerAdapter;Landroidx/viewpager/widget/PagerAdapter;)V"
            )
        ]
	    [InlineData
            (
			    "(Landroid/arch/persistence/db/SupportSQLiteOpenHelper$Callback;)Landroid/arch/persistence/db/SupportSQLiteOpenHelper$Configuration$Builder;",
			    "(Landroid/arch/persistence/db/SupportSQiteOpenHelper$Callback;)Landroidx/sqlite/db/SupportSQLiteOpenHelper$Configuration$Builder;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/arch/persistence/db/SupportSQLiteOpenHelper$Callback;)Landroid/arch/persistence/db/SupportSQLiteOpenHelper$Configuration$Builder;",
			    "(Landroid/arch/persistence/db/SupportSQiteOpenHelper$Callback;)Landroidx/sqlite/db/SupportSQLiteOpenHelper$Configuration$Builder;"
            )
        ]
	    [InlineData
            (
			    "(Landroid/arch/persistence/db/SupportSQLiteOpenHelper$Configuration;)Landroid/arch/persistence/db/SupportSQLiteOpenHelper;",
			    "(Landroid/arch/persistence/db/SupportSQiteOpenHelper$Configuration;)Landroidx/sqlite/db/SupportSQLiteOpenHelper;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/arch/persistence/db/SupportSQLiteOpenHelper$Configuration;)Landroid/arch/persistence/db/SupportSQLiteOpenHelper;",
			    "(Landroid/arch/persistence/db/SupportSQiteOpenHelper$Configuration;)Landroidx/sqlite/db/SupportSQLiteOpenHelper;"
            )
        ]
	    [InlineData
            (
			    "(Landroid/arch/persistence/room/DatabaseConfiguration;Landroid/arch/persistence/room/RoomOpenHelper$Delegate;Ljava/lang/String;)V",
			    "(Landroidx/room/DatabaseConfiguration;Landroidx/room/RoomOpenHelper$Delegate;Ljava/lang/String;)V"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/arch/persistence/room/DatabaseConfiguration;Landroid/arch/persistence/room/RoomOpenHelper$Delegate;Ljava/lang/String;Ljava/lang/String;)V",
			    "(Landroidx/room/DatabaseConfiguration;Landroidx/room/RoomOpenHelper$Delegate;Ljava/lang/String;Ljava/lang/String;)V"
            )
        ]
	    [InlineData
            (
			    "(Landroid/content/Context;Landroid/content/res/Resources;Lorg/xmlpull/v1/XmlPullParser;Landroid/util/AttributeSet;Landroid/content/res/Resources$Theme;)Landroid/support/graphics/drawable/AnimatedVectorDrawableCompat;",
			    "(Landroid/content/Context;Landroid/content/res/Resources;Lorg/xmlpull/v1/XmlPullParser;Landroid/util/AttributeSet;Landroid/content/res/Resources$Theme;)Landroidx/vectordrawable/graphics/drawable/AnimatedVectorDrawableCompat;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/graphics/drawable/Drawable;Landroid/support/graphics/drawable/Animatable2Compat$AnimationCallback;)V",
			    "(Landroid/graphics/drawable/Drawable;Landroidx/vectordrawable/graphics/drawable/Animatable2Compat$AnimationCallback;)V"
            )
        ]                        
	    [InlineData
            (
			    "(Landroid/graphics/drawable/Drawable;Landroid/support/graphics/drawable/Animatable2Compat$AnimationCallback;)Z",
			    "(Landroid/graphics/drawable/Drawable;Landroidx/vectordrawable/graphics/drawable/Animatable2Compat$AnimationCallback;)Z"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/graphics/drawable/Drawable;Landroid/support/graphics/drawable/Animatable2Compat$AnimationCallback;)Z",
			    "(Landroid/graphics/drawable/Drawable;Landroidx/vectordrawable/graphics/drawable/Animatable2Compat$AnimationCallback;)Z"
            )
        ]
	    [InlineData
            (
			    "(Landroid/content/res/Resources;ILandroid/content/res/Resources$Theme;)Landroid/support/graphics/drawable/VectorDrawableCompat;",
			    "(Landroid/content/res/Resources;ILandroid/content/res/Resources$Theme;)Landroidx/vectordrawable/graphics/drawable/VectorDrawableCompat;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/content/res/Resources;ILandroid/content/res/Resources$Theme;)Landroid/support/graphics/drawable/VectorDrawableCompat;",
			    "(Landroid/content/res/Resources;ILandroid/content/res/Resources$Theme;)Landroidx/vectordrawable/graphics/drawable/VectorDrawableCompat;"
            )
        ]
	    [InlineData
            (
			    "(Landroid/content/res/Resources;Lorg/xmlpull/v1/XmlPullParser;Landroid/util/AttributeSet;Landroid/content/res/Resources$Theme;)Landroid/support/graphics/drawable/VectorDrawableCompat;",
			    "(Landroid/content/res/Resources;Lorg/xmlpull/v1/XmlPullParser;Landroid/util/AttributeSet;Landroid/content/res/Resources$Theme;)Landroidx/vectordrawable/graphics/drawable/VectorDrawableCompat;"
            )
        ]
	    [InlineData
            (
			    "(Landroid/content/Context;Landroid/content/res/Resources;Lorg/xmlpull/v1/XmlPullParser;Landroid/util/AttributeSet;Landroid/content/res/Resources$Theme;)Landroid/support/v7/graphics/drawable/AnimatedStateListDrawableCompat;",
			    "(Landroid/content/Context;Landroid/content/res/Resources;Lorg/xmlpull/v1/XmlPullParser;Landroid/util/AttributeSet;Landroid/content/res/Resources$Theme;)Landroidx/appcompat/graphics/drawable/AnimatedStateListDrawableCompat;"
            )
        ]
	    [InlineData
            (
			    "(Landroid/content/Context;Landroid/content/res/Resources;Lorg/xmlpull/v1/XmlPullParser;Landroid/util/AttributeSet;Landroid/content/res/Resources$Theme;)Landroid/support/v7/graphics/drawable/AnimatedStateListDrawableCompat;",
			    "(Landroid/content/Context;Landroid/content/res/Resources;Lorg/xmlpull/v1/XmlPullParser;Landroid/util/AttributeSet;Landroid/content/res/Resources$Theme;)Landroidx/appcompat/graphics/drawable/AnimatedStateListDrawableCompat;"
            )
        ]
	    [InlineData
            (
			    "(Landroid/database/Cursor;Landroid/content/DialogInterface$OnClickListener;Ljava/lang/String;)Landroid/support/v7/app/AlertDialog$Builder;",
			    "(Landroid/database/Cursor;Landroid/content/DialogInterface$OnClickistener;Ljava/lang/String;)Landroidx/appcompat/app/AlertDialog$Builder;"
            )
        ]
	    [InlineData
            (
			    "([Ljava/lang/CharSequence;[ZLandroid/content/DialogInterface$OnMultiChoiceClickListener;)Landroid/support/v7/app/AlertDialog$Builder;",
			    "([Ljava/lang/CharSequence;[ZLandroid/content/DialogInterface$OnMultiChoiceClickListener;)Landroidx/appcompat/app/AlertDialog$Builder;"
            )
        ]
                        

	    [InlineData
            (
			    "([Ljava/lang/CharSequence;[ZLandroid/content/DialogInterface$OnMultiChoiceClickListener;)Landroid/support/v7/app/AlertDialog$Builder;",
			    "([Ljava/lang/CharSequence;[ZLandroid/content/DialogInterface$OnMultiChoiceClickListener;)Landroidx/appcompat/app/AlertDialog$Builder;"
            )
        ]
	    [InlineData
            (
			    "(Landroid/database/Cursor;Landroid/content/DialogInterface$OnClickListener;Ljava/lang/String;)Landroid/support/v7/app/AlertDialog$Builder;",
			    "(Landroid/database/Cursor;Landroid/content/DialogInterface$OnClickistener;Ljava/lang/String;)Landroidx/appcompat/app/AlertDialog$Builder;"
            )
        ]
	    [InlineData
            (
			    "(Landroid/database/Cursor;ILjava/lang/String;Landroid/content/DialogInterface$OnClickListener;)Landroid/support/v7/app/AlertDialog$Builder;",
			    "(Landroid/database/Cursor;ILjava/lang/String;Landroid/content/DialogInterface$OnClickistener;)Landroidx/appcompat/app/AlertDialog$Builder;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/database/Cursor;ILjava/lang/String;Landroid/content/DialogInterface$OnClickListener;)Landroid/support/v7/app/AlertDialog$Builder;",
			    "(Landroid/database/Cursor;ILjava/lang/String;Landroid/content/DialogInterface$OnClickistener;)Landroidx/appcompat/app/AlertDialog$Builder;"
            )
        ]
	    [InlineData
            (
			    "(Landroid/content/Context;Landroid/view/Window;Landroid/support/v7/app/AppCompatCallback;)Landroid/support/v7/app/AppCompatDelegate;",
			    "(Landroid/content/Context;Landroid/view/Window;Landroidx/appcompat/app/AppCompatCallback;)Landroidx/appcompat/app/AppCompatDelegate;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/content/Context;Landroid/view/Window;Landroid/support/v7/app/AppCompatCallback;)Landroid/support/v7/app/AppCompatDelegate;",
			    "(Landroid/content/Context;Landroid/view/Window;Landroidx/appcompat/app/AppCompatCallback;)Landroidx/appcompat/app/AppCompatDelegate;"
            )
        ]
	    [InlineData
            (
			    "(Landroid/content/Context;Landroid/util/AttributeSet;)Landroid/support/v7/widget/AppCompatImageView;",
			    "(Landroid/content/Context;Landroid/util/AttributeSet;)Landroidx/appcompat/widget/AppCompatImageView;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/content/Context;Landroid/util/AttributeSet;)Landroid/support/v7/widget/AppCompatImageView;",
			    "(Landroid/content/Context;Landroid/util/AttributeSet;)Landroidx/appcompat/widget/AppCompatImageView;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/content/Context;Landroid/util/AttributeSet;)Landroid/support/v7/widget/AppCompatMultiAutoCompleteTextView;",
			    "(Landroid/content/Context;Landroid/util/AttributeSet;)Landroidx/appcompat/widget/AppCompatMultiAutoCompleteTextView;"
            )
        ]
	    [InlineData
            (
			    "(Landroid/support/v4/view/ViewPropertyAnimatorCompat;Landroid/support/v4/view/ViewPropertyAnimatorCompat;)Landroid/support/v7/view/ViewPropertyAnimatorCompatSet;",
			    "(Landroidx/core/view/ViewPropertyAnimatorCompat;Landroidx/core/view/ViewPropertyAnimatorCompat;)Landroidx/appcompat/view/ViewPropertyAnimatorCompatSet;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/v4/view/ViewPropertyAnimatorCompat;Landroid/support/v4/view/ViewPropertyAnimatorCompat;)Landroid/support/v7/view/ViewPropertyAnimatorCompatSet;",
			    "(Landroidx/core/view/ViewPropertyAnimatorCompat;Landroidx/core/view/ViewPropertyAnimatorCompat;)Landroidx/appcompat/view/ViewPropertyAnimatorCompatSet;"
            )
        ]
	    [InlineData
            (
			    "(Landroid/graphics/Bitmap;Ljava/lang/String;Landroid/app/PendingIntent;Z)Landroid/support/customtabs/CustomTabsIntent$Builder;",
			    "(Landroid/graphics/Bitmap;Ljava/lang/String;Landroid/app/PendingIntent;Z)Landroidx/browser/customtabs/CustomTabsIntent$Builder;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/graphics/Bitmap;Ljava/lang/String;Landroid/app/PendingIntent;Z)Landroid/support/customtabs/CustomTabsIntent$Builder;",
			    "(Landroid/graphics/Bitmap;Ljava/lang/String;Landroid/app/PendingIntent;Z)Landroidx/browser/customtabs/CustomTabsIntent$Builder;"
            )
        ]
	    [InlineData
            (
			    "(Landroid/support/design/widget/CoordinatorLayout;Landroid/view/View;)Landroid/support/design/expandable/ExpandableWidget;",
			    "(Landroid/support/design/widget/Coordinatorayout;Landroid/view/View;)Lcom/google/android/material/expandable/ExpandableWidget;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/design/widget/CoordinatorLayout;Landroid/view/View;)Landroid/support/design/expandable/ExpandableWidget;",
			    "(Landroid/support/design/widget/Coordinatorayout;Landroid/view/View;)Lcom/google/android/material/expandable/ExpandableWidget;"
            )
        ]
	    [InlineData
            (
			    "(Landroid/support/design/shape/CornerTreatment;Landroid/support/design/shape/CornerTreatment;Landroid/support/design/shape/CornerTreatment;Landroid/support/design/shape/CornerTreatment;)V",
			    "(Lcom/google/android/material/shape/CornerTreatment;Lcom/google/android/material/shape/CornerTreatment;Lcom/google/android/material/shape/CornerTreatment;Lcom/google/android/material/shape/CornerTreatment;)V"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/design/shape/EdgeTreatment;Landroid/support/design/shape/EdgeTreatment;Landroid/support/design/shape/EdgeTreatment;Landroid/support/design/shape/EdgeTreatment;)V",
			    "(Lcom/google/android/material/shape/EdgeTreatment;Lcom/google/android/material/shape/EdgeTreatment;Lcom/google/android/material/shape/EdgeTreatment;Lcom/google/android/material/shape/EdgeTreatment;)V"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/design/shape/EdgeTreatment;Landroid/support/design/shape/EdgeTreatment;Landroid/support/design/shape/EdgeTreatment;Landroid/support/design/shape/EdgeTreatment;)V",
			    "(Lcom/google/android/material/shape/EdgeTreatment;Lcom/google/android/material/shape/EdgeTreatment;Lcom/google/android/material/shape/EdgeTreatment;Lcom/google/android/material/shape/EdgeTreatment;)V"
            )
        ]
	    [InlineData
            (
			    "(Landroid/graphics/drawable/Drawable;Landroid/content/res/ColorStateList;Landroid/graphics/PorterDuff$Mode;)Landroid/graphics/PorterDuffColorFilter;",
			    "(Landroid/graphics/drawable/Drawable;Landroid/content/res/ColorStateist;Landroid/graphics/PorterDuff$Mode;)Landroid/graphics/PorterDuffColorFilter;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/graphics/drawable/Drawable;Landroid/content/res/ColorStateList;Landroid/graphics/PorterDuff$Mode;)Landroid/graphics/PorterDuffColorFilter;",
			    "(Landroid/graphics/drawable/Drawable;Landroid/content/res/ColorStateist;Landroid/graphics/PorterDuff$Mode;)Landroid/graphics/PorterDuffColorFilter;"
            )
        ]
	    [InlineData
            (
			    "(Landroid/support/design/widget/CoordinatorLayout;Landroid/support/design/bottomappbar/BottomAppBar;Landroid/view/View;Landroid/view/View;II)Z",
			    "(Landroid/support/design/widget/Coordinatorayout;Lcom/google/android/material/bottomappbar/BottomAppBar;Landroid/view/View;Landroid/view/View;II)Z"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/design/widget/CoordinatorLayout;Landroid/support/design/bottomappbar/BottomAppBar;Landroid/view/View;Landroid/view/View;II)Z",
			    "(Landroid/support/design/widget/Coordinatorayout;Lcom/google/android/material/bottomappbar/BottomAppBar;Landroid/view/View;Landroid/view/View;II)Z"
            )
        ]
	    [InlineData
            (
			    "(FLandroid/support/design/circularreveal/CircularRevealWidget$RevealInfo;Landroid/support/design/circularreveal/CircularRevealWidget$RevealInfo;)Landroid/support/design/circularreveal/CircularRevealWidget$RevealInfo;",
			    "(FLcom/google/android/material/circularreveal/CircularRevealWidget$RevealInfo;Lcom/google/android/material/circularreveal/CircularRevealWidget$RevealInfo;)Lcom/google/android/material/circularreveal/CircularRevealWidget$RevealInfo;"
            )
        ]
                        

	    [InlineData
            (
			    "(FLandroid/support/design/circularreveal/CircularRevealWidget$RevealInfo;Landroid/support/design/circularreveal/CircularRevealWidget$RevealInfo;)Landroid/support/design/circularreveal/CircularRevealWidget$RevealInfo;",
			    "(FLcom/google/android/material/circularreveal/CircularRevealWidget$RevealInfo;Lcom/google/android/material/circularreveal/CircularRevealWidget$RevealInfo;)Lcom/google/android/material/circularreveal/CircularRevealWidget$RevealInfo;"
            )
        ]
	    [InlineData
            (
			    "(Landroid/support/design/widget/CoordinatorLayout;Landroid/support/design/widget/AppBarLayout;Landroid/os/Parcelable;)V",
			    "(Landroid/support/design/widget/Coordinatorayout;Landroid/support/design/widget/AppBarayout;Landroid/os/Parcelable;)V"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/design/widget/CoordinatorLayout;Landroid/support/design/widget/AppBarLayout;Landroid/os/Parcelable;)V",
			    "(Landroid/support/design/widget/Coordinatorayout;Landroid/support/design/widget/AppBarayout;Landroid/os/Parcelable;)V"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/design/widget/CoordinatorLayout;Landroid/support/design/widget/AppBarLayout;)Landroid/os/Parcelable;",
			    "(Landroid/support/design/widget/Coordinatorayout;Landroid/support/design/widget/AppBarayout;)Landroid/os/Parcelable;"
            )
        ]
	    [InlineData
            (
			    "(Landroid/support/design/widget/BaseTransientBottomBar$BaseCallback;)Landroid/support/design/widget/BaseTransientBottomBar;",
			    "(Lcom/google/android/material/snackbar/BaseTransientBottomBar$BaseCallback;)Lcom/google/android/material/snackbar/BaseTransientBottomBar;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/design/widget/BaseTransientBottomBar$BaseCallback;)Landroid/support/design/widget/BaseTransientBottomBar;",
			    "(Lcom/google/android/material/snackbar/BaseTransientBottomBar$BaseCallback;)Lcom/google/android/material/snackbar/BaseTransientBottomBar;"
            )
        ]
	    [InlineData
            (
			    "(Landroid/support/design/widget/CoordinatorLayout;Landroid/support/design/widget/FloatingActionButton;Landroid/graphics/Rect;)Z",
			    "(Landroid/support/design/widget/Coordinatorayout;Lcom/google/android/material/floatingactionbutton/FloatingActionButton;Landroid/graphics/Rect;)Z"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/design/widget/CoordinatorLayout;Landroid/support/design/widget/FloatingActionButton;Landroid/graphics/Rect;)Z",
			    "(Landroid/support/design/widget/Coordinatorayout;Lcom/google/android/material/floatingactionbutton/FloatingActionButton;Landroid/graphics/Rect;)Z"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/design/widget/CoordinatorLayout;Landroid/support/design/widget/FloatingActionButton;Landroid/view/View;)Z",
			    "(Landroid/support/design/widget/Coordinatorayout;Lcom/google/android/material/floatingactionbutton/FloatingActionButton;Landroid/view/View;)Z"
            )
        ]
	    [InlineData
            (
			    "(Landroid/view/ViewGroup;Landroid/support/transition/Transition;Landroid/support/transition/TransitionValues;Landroid/support/transition/TransitionValues;)J",
			    "(Landroid/view/ViewGroup;Landroidx/transition/Transition;Landroidx/transition/TransitionValues;Landroidx/transition/TransitionValues;)J"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/view/ViewGroup;Landroid/support/transition/Transition;Landroid/support/transition/TransitionValues;Landroid/support/transition/TransitionValues;)J",
			    "(Landroid/view/ViewGroup;Landroidx/transition/Transition;Landroidx/transition/TransitionValues;Landroidx/transition/TransitionValues;)J"
            )
        ]
	    [InlineData
            (
			    "(Landroid/view/ViewGroup;Landroid/support/transition/TransitionValues;Landroid/support/transition/TransitionValues;)Landroid/animation/Animator;",
			    "(Landroid/view/ViewGroup;Landroidx/transition/TransitionValues;Landroidx/transition/TransitionValues;)Landroid/animation/Animator;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/view/ViewGroup;Landroid/support/transition/TransitionValues;Landroid/support/transition/TransitionValues;)Landroid/animation/Animator;",
			    "(Landroid/view/ViewGroup;Landroidx/transition/TransitionValues;Landroidx/transition/TransitionValues;)Landroid/animation/Animator;"
            )
        ]
	    [InlineData
            (
			    "(Landroid/view/ViewGroup;Landroid/support/transition/Transition;Landroid/support/transition/TransitionValues;Landroid/support/transition/TransitionValues;)J",
			    "(Landroid/view/ViewGroup;Landroidx/transition/Transition;Landroidx/transition/TransitionValues;Landroidx/transition/TransitionValues;)J"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/view/ViewGroup;Landroid/support/transition/Transition;Landroid/support/transition/TransitionValues;Landroid/support/transition/TransitionValues;)J",
			    "(Landroid/view/ViewGroup;Landroidx/transition/Transition;Landroidx/transition/TransitionValues;Landroidx/transition/TransitionValues;)J"
            )
        ]
	    [InlineData
            (
			    "(Landroid/view/ViewGroup;Landroid/view/View;Landroid/support/transition/TransitionValues;Landroid/support/transition/TransitionValues;)Landroid/animation/Animator;",
			    "(Landroid/view/ViewGroup;Landroid/view/View;Landroidx/transition/TransitionValues;Landroidx/transition/TransitionValues;)Landroid/animation/Animator;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/view/ViewGroup;Landroid/view/View;Landroid/support/transition/TransitionValues;Landroid/support/transition/TransitionValues;)Landroid/animation/Animator;",
			    "(Landroid/view/ViewGroup;Landroid/view/View;Landroidx/transition/TransitionValues;Landroidx/transition/TransitionValues;)Landroid/animation/Animator;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/view/ViewGroup;Landroid/support/transition/TransitionValues;ILandroid/support/transition/TransitionValues;I)Landroid/animation/Animator;",
			    "(Landroid/view/ViewGroup;Landroidx/transition/TransitionValues;ILandroidx/transition/TransitionValues;I)Landroid/animation/Animator;"
            )
        ]
	    [InlineData
            (
			    "(Landroid/view/ViewGroup;Landroid/support/transition/Transition;Landroid/support/transition/TransitionValues;Landroid/support/transition/TransitionValues;)J",
			    "(Landroid/view/ViewGroup;Landroidx/transition/Transition;Landroidx/transition/TransitionValues;Landroidx/transition/TransitionValues;)J"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/view/ViewGroup;Landroid/support/transition/Transition;Landroid/support/transition/TransitionValues;Landroid/support/transition/TransitionValues;)J",
			    "(Landroid/view/ViewGroup;Landroidx/transition/Transition;Landroidx/transition/TransitionValues;Landroidx/transition/TransitionValues;)J"
            )
        ]
	    [InlineData
            (
			    "(Landroid/view/View;Landroid/support/v7/widget/RecyclerView$State;Landroid/support/v7/widget/RecyclerView$SmoothScroller$Action;)V",
			    "(Landroid/view/View;Landroidx/recyclerview/widget/RecyclerView$State;Landroidx/recyclerview/widget/RecyclerView$SmoothScroller$Action;)V"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/view/View;Landroid/support/v7/widget/RecyclerView$State;Landroid/support/v7/widget/RecyclerView$SmoothScroller$Action;)V",
			    "(Landroid/view/View;Landroidx/recyclerview/widget/RecyclerView$State;Landroidx/recyclerview/widget/RecyclerView$SmoothScroller$Action;)V"
            )
        ]
	    [InlineData
            (
			    "(Landroid/support/v7/widget/RecyclerView$ViewHolder;Landroid/support/v7/widget/RecyclerView$ItemAnimator$ItemHolderInfo;Landroid/support/v7/widget/RecyclerView$ItemAnimator$ItemHolderInfo;)Z",
			    "(Landroidx/recyclerview/widget/RecyclerView$ViewHolder;Landroidx/recyclerview/widget/RecyclerView$ItemAnimator$ItemHolderInfo;Landroidx/recyclerview/widget/RecyclerView$ItemAnimator$ItemHolderInfo;)Z"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/v7/widget/RecyclerView$ViewHolder;Landroid/support/v7/widget/RecyclerView$ViewHolder;Landroid/support/v7/widget/RecyclerView$ItemAnimator$ItemHolderInfo;Landroid/support/v7/widget/RecyclerView$ItemAnimator$ItemHolderInfo;)Z",
			    "(Landroidx/recyclerview/widget/RecyclerView$ViewHolder;Landroidx/recyclerview/widget/RecyclerView$ViewHolder;Landroidx/recyclerview/widget/RecyclerView$ItemAnimator$ItemHolderInfo;Landroidx/recyclerview/widget/RecyclerView$ItemAnimator$ItemHolderInfo;)Z"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/v7/widget/RecyclerView$ViewHolder;Landroid/support/v7/widget/RecyclerView$ItemAnimator$ItemHolderInfo;Landroid/support/v7/widget/RecyclerView$ItemAnimator$ItemHolderInfo;)Z",
			    "(Landroidx/recyclerview/widget/RecyclerView$ViewHolder;Landroidx/recyclerview/widget/RecyclerView$ItemAnimator$ItemHolderInfo;Landroidx/recyclerview/widget/RecyclerView$ItemAnimator$ItemHolderInfo;)Z"
            )
        ]
	    [InlineData
            (
			    "(Landroid/support/v7/widget/RecyclerView$ViewHolder;Landroid/support/v7/widget/RecyclerView$ViewHolder;Landroid/support/v7/widget/RecyclerView$ItemAnimator$ItemHolderInfo;Landroid/support/v7/widget/RecyclerView$ItemAnimator$ItemHolderInfo;)Z",
			    "(Landroidx/recyclerview/widget/RecyclerView$ViewHolder;Landroidx/recyclerview/widget/RecyclerView$ViewHolder;Landroidx/recyclerview/widget/RecyclerView$ItemAnimator$ItemHolderInfo;Landroidx/recyclerview/widget/RecyclerView$ItemAnimator$ItemHolderInfo;)Z"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/v7/widget/RecyclerView$ViewHolder;Landroid/support/v7/widget/RecyclerView$ItemAnimator$ItemHolderInfo;Landroid/support/v7/widget/RecyclerView$ItemAnimator$ItemHolderInfo;)Z",
			    "(Landroidx/recyclerview/widget/RecyclerView$ViewHolder;Landroidx/recyclerview/widget/RecyclerView$ItemAnimator$ItemHolderInfo;Landroidx/recyclerview/widget/RecyclerView$ItemAnimator$ItemHolderInfo;)Z"
            )
        ]
	    [InlineData
            (
			    "(Landroid/support/v7/widget/RecyclerView$Recycler;Landroid/support/v7/widget/RecyclerView$State;Landroid/support/v4/view/accessibility/AccessibilityNodeInfoCompat;)V",
			    "(Landroidx/recyclerview/widget/RecyclerView$Recycler;Landroidx/recyclerview/widget/RecyclerView$State;Landroidx/core/view/accessibility/AccessibilityNodeInfoCompat;)V"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/v7/widget/RecyclerView$Recycler;Landroid/support/v7/widget/RecyclerView$State;Landroid/view/View;Landroid/support/v4/view/accessibility/AccessibilityNodeInfoCompat;)V",
			    "(Landroidx/recyclerview/widget/RecyclerView$Recycler;Landroidx/recyclerview/widget/RecyclerView$State;Landroid/view/View;Landroidx/core/view/accessibility/AccessibilityNodeInfoCompat;)V"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/v7/widget/RecyclerView$Recycler;Landroid/support/v7/widget/RecyclerView$State;Landroid/view/View;Landroid/support/v4/view/accessibility/AccessibilityNodeInfoCompat;)V",
			    "(Landroidx/recyclerview/widget/RecyclerView$Recycler;Landroidx/recyclerview/widget/RecyclerView$State;Landroid/view/View;Landroidx/core/view/accessibility/AccessibilityNodeInfoCompat;)V"
            )
        ]
	    [InlineData
            (
			    "(Landroid/support/v7/widget/RecyclerView;Landroid/support/v7/widget/RecyclerView$State;Landroid/view/View;Landroid/view/View;)Z",
			    "(Landroidx/recyclerview/widget/RecyclerView;Landroidx/recyclerview/widget/RecyclerView$State;Landroid/view/View;Landroid/view/View;)Z"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/v7/widget/RecyclerView;Landroid/support/v7/widget/RecyclerView$State;Landroid/view/View;Landroid/view/View;)Z",
			    "(Landroidx/recyclerview/widget/RecyclerView;Landroidx/recyclerview/widget/RecyclerView$State;Landroid/view/View;Landroid/view/View;)Z"
            )
        ]
	    [InlineData
            (
			    "(Landroid/view/View;Landroid/support/v7/widget/RecyclerView$State;Landroid/support/v7/widget/RecyclerView$SmoothScroller$Action;)V",
			    "(Landroid/view/View;Landroidx/recyclerview/widget/RecyclerView$State;Landroidx/recyclerview/widget/RecyclerView$SmoothScroller$Action;)V"
            )
        ]
	    [InlineData
            (
			    "(Landroid/support/v7/widget/RecyclerView$ViewHolder;Landroid/support/v7/widget/RecyclerView$ViewHolder;Landroid/support/v7/widget/RecyclerView$ItemAnimator$ItemHolderInfo;Landroid/support/v7/widget/RecyclerView$ItemAnimator$ItemHolderInfo;)Z",
			    "(Landroidx/recyclerview/widget/RecyclerView$ViewHolder;Landroidx/recyclerview/widget/RecyclerView$ViewHolder;Landroidx/recyclerview/widget/RecyclerView$ItemAnimator$ItemHolderInfo;Landroidx/recyclerview/widget/RecyclerView$ItemAnimator$ItemHolderInfo;)Z"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/v7/widget/RecyclerView$ViewHolder;Landroid/support/v7/widget/RecyclerView$ViewHolder;Landroid/support/v7/widget/RecyclerView$ItemAnimator$ItemHolderInfo;Landroid/support/v7/widget/RecyclerView$ItemAnimator$ItemHolderInfo;)Z",
			    "(Landroidx/recyclerview/widget/RecyclerView$ViewHolder;Landroidx/recyclerview/widget/RecyclerView$ViewHolder;Landroidx/recyclerview/widget/RecyclerView$ItemAnimator$ItemHolderInfo;Landroidx/recyclerview/widget/RecyclerView$ItemAnimator$ItemHolderInfo;)Z"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/v7/widget/RecyclerView$ViewHolder;Landroid/support/v7/widget/RecyclerView$ViewHolder;IIII)Z",
			    "(Landroidx/recyclerview/widget/RecyclerView$ViewHolder;Landroidx/recyclerview/widget/RecyclerView$ViewHolder;IIII)Z"
            )
        ]
	    [InlineData
            (
			    "(Landroid/support/v7/widget/RecyclerView;Landroid/support/v7/widget/RecyclerView$ViewHolder;Landroid/support/v7/widget/RecyclerView$ViewHolder;)Z",
			    "(Landroidx/recyclerview/widget/RecyclerView;Landroidx/recyclerview/widget/RecyclerView$ViewHolder;Landroidx/recyclerview/widget/RecyclerView$ViewHolder;)Z"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/v7/widget/RecyclerView;Landroid/support/v7/widget/RecyclerView$ViewHolder;Landroid/support/v7/widget/RecyclerView$ViewHolder;)Z",
			    "(Landroidx/recyclerview/widget/RecyclerView;Landroidx/recyclerview/widget/RecyclerView$ViewHolder;Landroidx/recyclerview/widget/RecyclerView$ViewHolder;)Z"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/v7/widget/RecyclerView$ViewHolder;Ljava/util/List;II)Landroid/support/v7/widget/RecyclerView$ViewHolder;",
			    "(Landroidx/recyclerview/widget/RecyclerView$ViewHolder;Ljava/util/ist;II)Landroidx/recyclerview/widget/RecyclerView$ViewHolder;"
            )
        ]
	    [InlineData
            (
			    "(Landroid/support/v7/widget/RecyclerView;Landroid/support/v7/widget/RecyclerView$ViewHolder;Landroid/support/v7/widget/RecyclerView$ViewHolder;)Z",
			    "(Landroidx/recyclerview/widget/RecyclerView;Landroidx/recyclerview/widget/RecyclerView$ViewHolder;Landroidx/recyclerview/widget/RecyclerView$ViewHolder;)Z"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/v7/widget/RecyclerView;Landroid/support/v7/widget/RecyclerView$ViewHolder;ILandroid/support/v7/widget/RecyclerView$ViewHolder;III)V",
			    "(Landroidx/recyclerview/widget/RecyclerView;Landroidx/recyclerview/widget/RecyclerView$ViewHolder;ILandroidx/recyclerview/widget/RecyclerView$ViewHolder;III)V"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/v7/widget/RecyclerView;Landroid/support/v7/widget/RecyclerView$ViewHolder;ILandroid/support/v7/widget/RecyclerView$ViewHolder;III)V",
			    "(Landroidx/recyclerview/widget/RecyclerView;Landroidx/recyclerview/widget/RecyclerView$ViewHolder;ILandroidx/recyclerview/widget/RecyclerView$ViewHolder;III)V"
            )
        ]
	    [InlineData
            (
			    "(Landroid/support/v7/widget/RecyclerView;Landroid/support/v7/widget/RecyclerView$ViewHolder;Landroid/support/v7/widget/RecyclerView$ViewHolder;)Z",
			    "(Landroidx/recyclerview/widget/RecyclerView;Landroidx/recyclerview/widget/RecyclerView$ViewHolder;Landroidx/recyclerview/widget/RecyclerView$ViewHolder;)Z"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/v7/widget/RecyclerView;Landroid/support/v7/widget/RecyclerView$ViewHolder;Landroid/support/v7/widget/RecyclerView$ViewHolder;)Z",
			    "(Landroidx/recyclerview/widget/RecyclerView;Landroidx/recyclerview/widget/RecyclerView$ViewHolder;Landroidx/recyclerview/widget/RecyclerView$ViewHolder;)Z"
            )
        ]
	    [InlineData
            (
			    "(Landroid/support/v7/widget/RecyclerView;Landroid/support/v7/widget/RecyclerView$ViewHolder;Landroid/support/v7/widget/RecyclerView$ViewHolder;)Z",
			    "(Landroidx/recyclerview/widget/RecyclerView;Landroidx/recyclerview/widget/RecyclerView$ViewHolder;Landroidx/recyclerview/widget/RecyclerView$ViewHolder;)Z"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/v7/widget/RecyclerView;Landroid/support/v7/widget/RecyclerView$ViewHolder;Landroid/support/v7/widget/RecyclerView$ViewHolder;)Z",
			    "(Landroidx/recyclerview/widget/RecyclerView;Landroidx/recyclerview/widget/RecyclerView$ViewHolder;Landroidx/recyclerview/widget/RecyclerView$ViewHolder;)Z"
            )
        ]
	    [InlineData
            (
			    "(Landroid/support/v4/media/session/MediaSessionCompat$Token;)Landroid/support/v4/media/app/NotificationCompat$MediaStyle;",
			    "(Landroid/support/v4/media/session/MediaSessionCompat$Token;)Landroidx/media/app/NotificationCompat$MediaStyle;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/v4/media/session/MediaSessionCompat$Token;)Landroid/support/v4/media/app/NotificationCompat$MediaStyle;",
			    "(Landroid/support/v4/media/session/MediaSessionCompat$Token;)Landroidx/media/app/NotificationCompat$MediaStyle;"
            )
        ]
	    [InlineData
            (
			    "(Landroid/support/v17/leanback/util/StateMachine$State;Landroid/support/v17/leanback/util/StateMachine$State;Landroid/support/v17/leanback/util/StateMachine$Condition;)V",
			    "(Landroidx/leanback/util/StateMachine$State;Landroidx/leanback/util/StateMachine$State;Landroidx/leanback/util/StateMachine$Condition;)V"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/v17/leanback/util/StateMachine$State;Landroid/support/v17/leanback/util/StateMachine$State;Landroid/support/v17/leanback/util/StateMachine$Condition;)V",
			    "(Landroidx/leanback/util/StateMachine$State;Landroidx/leanback/util/StateMachine$State;Landroidx/leanback/util/StateMachine$Condition;)V"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/v17/leanback/util/StateMachine$State;Landroid/support/v17/leanback/util/StateMachine$State;Landroid/support/v17/leanback/util/StateMachine$Event;)V",
			    "(Landroidx/leanback/util/StateMachine$State;Landroidx/leanback/util/StateMachine$State;Landroidx/leanback/util/StateMachine$Event;)V"
            )
        ]
	    [InlineData
            (
			    "(Landroid/support/v17/leanback/widget/Presenter$ViewHolder;Ljava/lang/Object;Landroid/support/v17/leanback/widget/RowPresenter$ViewHolder;Landroid/support/v17/leanback/widget/Row;)V",
			    "(Landroidx/leanback/widget/Presenter$ViewHolder;Ljava/lang/Object;Landroidx/leanback/widget/RowPresenter$ViewHolder;Landroidx/leanback/widget/Row;)V"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/v17/leanback/widget/Presenter$ViewHolder;Ljava/lang/Object;Landroid/support/v17/leanback/widget/RowPresenter$ViewHolder;Landroid/support/v17/leanback/widget/Row;)V",
			    "(Landroidx/leanback/widget/Presenter$ViewHolder;Ljava/lang/Object;Landroidx/leanback/widget/RowPresenter$ViewHolder;Landroidx/leanback/widget/Row;)V"
            )
        ]
	    [InlineData
            (
			    "(Landroid/support/v17/leanback/widget/Presenter$ViewHolder;Ljava/lang/Object;Landroid/support/v17/leanback/widget/RowPresenter$ViewHolder;Landroid/support/v17/leanback/widget/Row;)V",
			    "(Landroidx/leanback/widget/Presenter$ViewHolder;Ljava/lang/Object;Landroidx/leanback/widget/RowPresenter$ViewHolder;Landroidx/leanback/widget/Row;)V"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/v17/leanback/widget/Presenter$ViewHolder;Ljava/lang/Object;Landroid/support/v17/leanback/widget/RowPresenter$ViewHolder;Landroid/support/v17/leanback/widget/Row;)V",
			    "(Landroidx/leanback/widget/Presenter$ViewHolder;Ljava/lang/Object;Landroidx/leanback/widget/RowPresenter$ViewHolder;Landroidx/leanback/widget/Row;)V"
            )
        ]
	    [InlineData
            (
			    "(Landroid/graphics/drawable/Drawable;Landroid/graphics/drawable/Drawable;Landroid/support/v17/leanback/widget/ParallaxTarget$PropertyValuesHolderTarget;)V",
			    "(Landroid/graphics/drawable/Drawable;Landroid/graphics/drawable/Drawable;Landroidx/leanback/widget/ParallaxTarget$PropertyValuesHolderTarget;)V"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/graphics/drawable/Drawable;Landroid/graphics/drawable/Drawable;Landroid/support/v17/leanback/widget/ParallaxTarget$PropertyValuesHolderTarget;)V",
			    "(Landroid/graphics/drawable/Drawable;Landroid/graphics/drawable/Drawable;Landroidx/leanback/widget/ParallaxTarget$PropertyValuesHolderTarget;)V"
            )
        ]
                        

	    [InlineData
            (
			    "()Landroid/support/v17/leanback/media/PlaybackGlueHost;",
			    "()Landroidx/leanback/media/PlaybackGlueHost;"
            )
        ]
	    [InlineData
            (
			    "(Landroid/support/v17/leanback/widget/FullWidthDetailsOverviewRowPresenter;Landroid/support/v17/leanback/widget/FullWidthDetailsOverviewRowPresenter$ViewHolder;III)V",
			    "(Landroidx/leanback/widget/FullWidthDetailsOverviewRowPresenter;Landroidx/leanback/widget/FullWidthDetailsOverviewRowPresenter$ViewHolder;III)V"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/v17/leanback/widget/FullWidthDetailsOverviewRowPresenter;Landroid/support/v17/leanback/widget/FullWidthDetailsOverviewRowPresenter$ViewHolder;III)V",
			    "(Landroidx/leanback/widget/FullWidthDetailsOverviewRowPresenter;Landroidx/leanback/widget/FullWidthDetailsOverviewRowPresenter$ViewHolder;III)V"
            )
        ]
	    [InlineData
            (
			    "(Landroid/graphics/drawable/Drawable;Landroid/graphics/drawable/Drawable;Landroid/support/v17/leanback/widget/ParallaxTarget$PropertyValuesHolderTarget;)V",
			    "(Landroid/graphics/drawable/Drawable;Landroid/graphics/drawable/Drawable;Landroidx/leanback/widget/ParallaxTarget$PropertyValuesHolderTarget;)V"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/graphics/drawable/Drawable;Landroid/graphics/drawable/Drawable;Landroid/support/v17/leanback/widget/ParallaxTarget$PropertyValuesHolderTarget;)V",
			    "(Landroid/graphics/drawable/Drawable;Landroid/graphics/drawable/Drawable;Landroidx/leanback/widget/ParallaxTarget$PropertyValuesHolderTarget;)V"
            )
        ]
	    [InlineData
            (
			    "()Landroid/support/v17/leanback/widget/BaseOnItemViewClickedListener;",
			    "()Landroidx/leanback/widget/BaseOnItemViewClickedListener;"
            )
        ]
                        

	    [InlineData
            (
			    "()Landroid/support/v17/leanback/widget/BaseOnItemViewClickedListener;",
			    "()Landroidx/leanback/widget/BaseOnItemViewClickedListener;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/v17/leanback/widget/BaseOnItemViewClickedListener;)V",
			    "(Landroid/support/v17/leanback/widget/BaseOnItemViewClickedistener;)V"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/v17/leanback/widget/BaseOnItemViewClickedListener;)V",
			    "(Landroid/support/v17/leanback/widget/BaseOnItemViewClickedistener;)V"
            )
        ]
	    [InlineData
            (
			    "()Landroid/support/v17/leanback/app/BrowseSupportFragment$MainFragmentAdapter;",
			    "()Landroidx/leanback/app/BrowseSupportFragment$MainFragmentAdapter;"
            )
        ]
                        

	    [InlineData
            (
			    "()Landroid/support/v17/leanback/app/BrowseSupportFragment$MainFragmentRowsAdapter;",
			    "()Landroidx/leanback/app/BrowseSupportFragment$MainFragmentRowsAdapter;"
            )
        ]
                        

	    [InlineData
            (
			    "()Landroid/support/v17/leanback/app/BrowseSupportFragment$MainFragmentRowsAdapter;",
			    "()Landroidx/leanback/app/BrowseSupportFragment$MainFragmentRowsAdapter;"
            )
        ]
	    [InlineData
            (
			    "(Ljava/lang/Class;Landroid/support/v17/leanback/widget/Presenter;)Landroid/support/v17/leanback/widget/ClassPresenterSelector;",
			    "(Ljava/lang/Class;Landroidx/leanback/widget/Presenter;)Landroidx/leanback/widget/ClassPresenterSelector;"
            )
        ]
                        

	    [InlineData
            (
			    "(Ljava/lang/Class;Landroid/support/v17/leanback/widget/Presenter;)Landroid/support/v17/leanback/widget/ClassPresenterSelector;",
			    "(Ljava/lang/Class;Landroidx/leanback/widget/Presenter;)Landroidx/leanback/widget/ClassPresenterSelector;"
            )
        ]
                        

	    [InlineData
            (
			    "(Ljava/lang/Class;Landroid/support/v17/leanback/widget/PresenterSelector;)Landroid/support/v17/leanback/widget/ClassPresenterSelector;",
			    "(Ljava/lang/Class;Landroidx/leanback/widget/PresenterSelector;)Landroidx/leanback/widget/ClassPresenterSelector;"
            )
        ]
                        

	    [InlineData
            (
			    "(Ljava/lang/Class;Landroid/support/v17/leanback/widget/PresenterSelector;)Landroid/support/v17/leanback/widget/ClassPresenterSelector;",
			    "(Ljava/lang/Class;Landroidx/leanback/widget/PresenterSelector;)Landroidx/leanback/widget/ClassPresenterSelector;"
            )
        ]
	    [InlineData
            (
			    "(Landroid/support/v17/leanback/widget/DetailsOverviewLogoPresenter$ViewHolder;Landroid/support/v17/leanback/widget/FullWidthDetailsOverviewRowPresenter$ViewHolder;Landroid/support/v17/leanback/widget/FullWidthDetailsOverviewRowPresenter;)V",
			    "(Landroid/support/v17/leanback/widget/DetailsOverviewogoPresenter$ViewHolder;Landroidx/leanback/widget/FullWidthDetailsOverviewRowPresenter$ViewHolder;Landroidx/leanback/widget/FullWidthDetailsOverviewRowPresenter;)V"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/v17/leanback/widget/DetailsOverviewLogoPresenter$ViewHolder;Landroid/support/v17/leanback/widget/FullWidthDetailsOverviewRowPresenter$ViewHolder;Landroid/support/v17/leanback/widget/FullWidthDetailsOverviewRowPresenter;)V",
			    "(Landroid/support/v17/leanback/widget/DetailsOverviewogoPresenter$ViewHolder;Landroidx/leanback/widget/FullWidthDetailsOverviewRowPresenter$ViewHolder;Landroidx/leanback/widget/FullWidthDetailsOverviewRowPresenter;)V"
            )
        ]
	    [InlineData
            (
			    "(Landroid/content/Context;Landroid/support/v17/leanback/widget/DetailsParallax;Landroid/graphics/drawable/Drawable;Landroid/graphics/drawable/Drawable;Landroid/support/v17/leanback/widget/ParallaxTarget;)V",
			    "(Landroid/content/Context;Landroidx/leanback/widget/DetailsParallax;Landroid/graphics/drawable/Drawable;Landroid/graphics/drawable/Drawable;Landroidx/leanback/widget/ParallaxTarget;)V"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/content/Context;Landroid/support/v17/leanback/widget/DetailsParallax;Landroid/graphics/drawable/Drawable;Landroid/support/v17/leanback/widget/ParallaxTarget;)V",
			    "(Landroid/content/Context;Landroidx/leanback/widget/DetailsParallax;Landroid/graphics/drawable/Drawable;Landroidx/leanback/widget/ParallaxTarget;)V"
            )
        ]
	    [InlineData
            (
			    "(Landroid/support/v17/leanback/widget/FullWidthDetailsOverviewRowPresenter;Landroid/view/View;Landroid/support/v17/leanback/widget/Presenter;Landroid/support/v17/leanback/widget/DetailsOverviewLogoPresenter;)V",
			    "(Landroidx/leanback/widget/FullWidthDetailsOverviewRowPresenter;Landroid/view/View;Landroidx/leanback/widget/Presenter;Landroid/support/v17/leanback/widget/DetailsOverviewogoPresenter;)V"
            )
        ]
	    [InlineData
            (
			    "(Landroid/view/LayoutInflater;Landroid/view/ViewGroup;Landroid/support/v17/leanback/widget/GuidanceStylist$Guidance;)Landroid/view/View;",
			    "(Landroid/view/ayoutInflater;Landroid/view/ViewGroup;Landroidx/leanback/widget/GuidanceStylist$Guidance;)Landroid/view/View;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/view/LayoutInflater;Landroid/view/ViewGroup;Landroid/support/v17/leanback/widget/GuidanceStylist$Guidance;)Landroid/view/View;",
			    "(Landroid/view/ayoutInflater;Landroid/view/ViewGroup;Landroidx/leanback/widget/GuidanceStylist$Guidance;)Landroid/view/View;"
            )
        ]
	    [InlineData
            (
			    "(Ljava/util/List;Landroid/support/v17/leanback/widget/GuidedActionAdapter$ClickListener;Landroid/support/v17/leanback/widget/GuidedActionAdapter$FocusListener;Landroid/support/v17/leanback/widget/GuidedActionsStylist;Z)V",
			    "(Ljava/util/ist;Landroidx/leanback/widget/GuidedActionAdapter$Clickistener;Landroidx/leanback/widget/GuidedActionAdapter$Focusistener;Landroidx/leanback/widget/GuidedActionsStylist;Z)V"
            )
        ]
	    [InlineData
            (
			    "(Landroid/support/v17/leanback/widget/Presenter$ViewHolder;Ljava/lang/Object;Landroid/support/v17/leanback/widget/RowPresenter$ViewHolder;Ljava/lang/Object;)V",
			    "(Landroidx/leanback/widget/Presenter$ViewHolder;Ljava/lang/Object;Landroidx/leanback/widget/RowPresenter$ViewHolder;Ljava/lang/Object;)V"
            )
        ]
	    [InlineData
            (
			    "(Landroid/support/v17/leanback/widget/Presenter$ViewHolder;Ljava/lang/Object;Landroid/support/v17/leanback/widget/RowPresenter$ViewHolder;Landroid/support/v17/leanback/widget/Row;)V",
			    "(Landroidx/leanback/widget/Presenter$ViewHolder;Ljava/lang/Object;Landroidx/leanback/widget/RowPresenter$ViewHolder;Landroidx/leanback/widget/Row;)V"
            )
        ]
	    [InlineData
            (
			    "(Landroid/support/v17/leanback/widget/ParallaxTarget;)Landroid/support/v17/leanback/widget/ParallaxEffect;",
			    "(Landroidx/leanback/widget/ParallaxTarget;)Landroidx/leanback/widget/ParallaxEffect;"
            )
        ]
                        

	    [InlineData
            (
			    "(Ljava/lang/Object;Landroid/animation/PropertyValuesHolder;)Landroid/support/v17/leanback/widget/ParallaxEffect;",
			    "(Ljava/lang/Object;Landroid/animation/PropertyValuesHolder;)Landroidx/leanback/widget/ParallaxEffect;"
            )
        ]
                        

	    [InlineData
            (
			    "(Ljava/lang/Object;Landroid/animation/PropertyValuesHolder;)Landroid/support/v17/leanback/widget/ParallaxEffect;",
			    "(Ljava/lang/Object;Landroid/animation/PropertyValuesHolder;)Landroidx/leanback/widget/ParallaxEffect;"
            )
        ]
	    [InlineData
            (
			    "(Landroid/support/v17/leanback/widget/PlaybackTransportRowPresenter;Landroid/view/View;Landroid/support/v17/leanback/widget/Presenter;)V",
			    "(Landroidx/leanback/widget/PlaybackTransportRowPresenter;Landroid/view/View;Landroidx/leanback/widget/Presenter;)V"
            )
        ]
	    [InlineData
            (
			    "(Landroid/support/v17/leanback/widget/ShadowOverlayHelper$Options;)Landroid/support/v17/leanback/widget/ShadowOverlayHelper$Builder;",
			    "(Landroidx/leanback/widget/ShadowOverlayHelper$Options;)Landroidx/leanback/widget/ShadowOverlayHelper$Builder;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/v17/leanback/widget/ShadowOverlayHelper$Options;)Landroid/support/v17/leanback/widget/ShadowOverlayHelper$Builder;",
			    "(Landroidx/leanback/widget/ShadowOverlayHelper$Options;)Landroidx/leanback/widget/ShadowOverlayHelper$Builder;"
            )
        ]
	    [InlineData
            (
			    "(Landroid/support/v7/media/MediaRouteProvider;Landroid/support/v7/media/MediaRouteProviderDescriptor;)V",
			    "(Landroidx/mediarouter/media/MediaRouteProvider;Landroidx/mediarouter/media/MediaRouteProviderDescriptor;)V"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/v7/media/MediaRouteProvider;Landroid/support/v7/media/MediaRouteProviderDescriptor;)V",
			    "(Landroidx/mediarouter/media/MediaRouteProvider;Landroidx/mediarouter/media/MediaRouteProviderDescriptor;)V"
            )
        ]
	    [InlineData
            (
			    "(Landroid/support/v7/media/MediaRouter;Landroid/support/v7/media/MediaRouter$RouteInfo;)V",
			    "(Landroidx/mediarouter/media/MediaRouter;Landroidx/mediarouter/media/MediaRouter$RouteInfo;)V"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/v7/media/MediaRouter;Landroid/support/v7/media/MediaRouter$RouteInfo;)V",
			    "(Landroidx/mediarouter/media/MediaRouter;Landroidx/mediarouter/media/MediaRouter$RouteInfo;)V"
            )
        ]
	    [InlineData
            (
			    "(Landroid/os/Bundle;Landroid/support/v7/media/RemotePlaybackClient$SessionActionCallback;)V",
			    "(Landroid/os/Bundle;Landroidx/mediarouter/media/RemotePlaybackClient$SessionActionCallback;)V"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/net/Uri;Ljava/lang/String;Landroid/os/Bundle;JLandroid/os/Bundle;Landroid/support/v7/media/RemotePlaybackClient$ItemActionCallback;)V",
			    "(Landroid/net/Uri;Ljava/lang/String;Landroid/os/Bundle;JLandroid/os/Bundle;Landroidx/mediarouter/media/RemotePlaybackClient$ItemActionCallback;)V"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/net/Uri;Ljava/lang/String;Landroid/os/Bundle;JLandroid/os/Bundle;Landroid/support/v7/media/RemotePlaybackClient$ItemActionCallback;)V",
			    "(Landroid/net/Uri;Ljava/lang/String;Landroid/os/Bundle;JLandroid/os/Bundle;Landroidx/mediarouter/media/RemotePlaybackClient$ItemActionCallback;)V"
            )
        ]
                        

	    [InlineData
            (
			    "(Ljava/lang/String;Landroid/os/Bundle;Landroid/support/v7/media/RemotePlaybackClient$ItemActionCallback;)V",
			    "(Ljava/lang/String;Landroid/os/Bundle;Landroidx/mediarouter/media/RemotePlaybackClient$ItemActionCallback;)V"
            )
        ]
	    [InlineData
            (
			    "(Landroid/os/Bundle;Ljava/lang/String;Landroid/support/v7/media/MediaSessionStatus;Ljava/lang/String;Landroid/support/v7/media/MediaItemStatus;)V",
			    "(Landroid/os/Bundle;Ljava/lang/String;Landroidx/mediarouter/media/MediaSessionStatus;Ljava/lang/String;Landroidx/mediarouter/media/MediaItemStatus;)V"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/os/Bundle;Ljava/lang/String;Landroid/support/v7/media/MediaSessionStatus;Ljava/lang/String;Landroid/support/v7/media/MediaItemStatus;)V",
			    "(Landroid/os/Bundle;Ljava/lang/String;Landroidx/mediarouter/media/MediaSessionStatus;Ljava/lang/String;Landroidx/mediarouter/media/MediaItemStatus;)V"
            )
        ]
 	    [InlineData
            (
			    "(Landroid/os/Bundle;Ljava/lang/String;Landroid/support/v7/media/MediaSessionStatus;Ljava/lang/String;Landroid/support/v7/media/MediaItemStatus;)V",
			    "(Landroid/os/Bundle;Ljava/lang/String;Landroidx/mediarouter/media/MediaSessionStatus;Ljava/lang/String;Landroidx/mediarouter/media/MediaItemStatus;)V"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/os/Bundle;Ljava/lang/String;Landroid/support/v7/media/MediaSessionStatus;Ljava/lang/String;Landroid/support/v7/media/MediaItemStatus;)V",
			    "(Landroid/os/Bundle;Ljava/lang/String;Landroidx/mediarouter/media/MediaSessionStatus;Ljava/lang/String;Landroidx/mediarouter/media/MediaItemStatus;)V"
            )
        ]
	    [InlineData
            (
			    "(Landroid/support/v17/preference/LeanbackListPreferenceDialogFragment;[Ljava/lang/CharSequence;[Ljava/lang/CharSequence;Ljava/util/Set;)V",
			    "(Landroid/support/v17/preference/eanbackistPreferenceDialogFragment;[Ljava/lang/CharSequence;[Ljava/lang/CharSequence;Ljava/util/Set;)V"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/v17/preference/LeanbackListPreferenceDialogFragment;[Ljava/lang/CharSequence;[Ljava/lang/CharSequence;Ljava/util/Set;)V",
			    "(Landroid/support/v17/preference/eanbackistPreferenceDialogFragment;[Ljava/lang/CharSequence;[Ljava/lang/CharSequence;Ljava/util/Set;)V"
            )
        ]
	    [InlineData
            (
			    "(Landroid/view/LayoutInflater;Landroid/view/ViewGroup;Landroid/os/Bundle;)Landroid/support/v7/widget/RecyclerView;",
			    "(Landroid/view/ayoutInflater;Landroid/view/ViewGroup;Landroid/os/Bundle;)Landroidx/recyclerview/widget/RecyclerView;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/view/LayoutInflater;Landroid/view/ViewGroup;Landroid/os/Bundle;)Landroid/support/v7/widget/RecyclerView;",
			    "(Landroid/view/ayoutInflater;Landroid/view/ViewGroup;Landroid/os/Bundle;)Landroidx/recyclerview/widget/RecyclerView;"
            )
        ]
	    [InlineData
            (
			    "(Landroid/view/LayoutInflater;Landroid/view/ViewGroup;Landroid/os/Bundle;)Landroid/support/v7/widget/RecyclerView;",
			    "(Landroid/view/ayoutInflater;Landroid/view/ViewGroup;Landroid/os/Bundle;)Landroidx/recyclerview/widget/RecyclerView;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/view/LayoutInflater;Landroid/view/ViewGroup;Landroid/os/Bundle;)Landroid/support/v7/widget/RecyclerView;",
			    "(Landroid/view/ayoutInflater;Landroid/view/ViewGroup;Landroid/os/Bundle;)Landroidx/recyclerview/widget/RecyclerView;"
            )
        ]
	    [InlineData
            (
			    "(Landroid/content/Context;ILandroid/support/v7/preference/PreferenceScreen;)Landroid/support/v7/preference/PreferenceScreen;",
			    "(Landroid/content/Context;ILandroidx/preference/PreferenceScreen;)Landroidx/preference/PreferenceScreen;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/content/Context;ILandroid/support/v7/preference/PreferenceScreen;)Landroid/support/v7/preference/PreferenceScreen;",
			    "(Landroid/content/Context;ILandroidx/preference/PreferenceScreen;)Landroidx/preference/PreferenceScreen;"
            )
        ]
	    [InlineData
            (
			    "(Ljava/lang/String;Landroid/support/v7/widget/RecyclerView;Landroidx/recyclerview/selection/ItemKeyProvider;Landroidx/recyclerview/selection/ItemDetailsLookup;Landroidx/recyclerview/selection/StorageStrategy;)V",
			    "(Ljava/lang/String;Landroidx/recyclerview/widget/RecyclerView;Landroidx/recyclerview/selection/ItemKeyProvider;Landroidx/recyclerview/selection/ItemDetailsookup;Landroidx/recyclerview/selection/StorageStrategy;)V"
            )
        ]
	    [InlineData
            (
			    "(Landroid/app/PendingIntent;Landroid/support/v4/graphics/drawable/IconCompat;ILjava/lang/CharSequence;)V",
			    "(Landroid/app/PendingIntent;Landroidx/core/graphics/drawable/IconCompat;ILjava/lang/CharSequence;)V"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/app/PendingIntent;Landroid/support/v4/graphics/drawable/IconCompat;Ljava/lang/CharSequence;)V",
			    "(Landroid/app/PendingIntent;Landroidx/core/graphics/drawable/IconCompat;Ljava/lang/CharSequence;)V"
            )
        ]
	    [InlineData
            (
			    "(Landroid/app/PendingIntent;Landroid/support/v4/graphics/drawable/IconCompat;Ljava/lang/CharSequence;Z)Landroidx/slice/builders/SliceAction;",
			    "(Landroid/app/PendingIntent;Landroidx/core/graphics/drawable/IconCompat;Ljava/lang/CharSequence;Z)Landroidx/slice/builders/SliceAction;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/app/PendingIntent;Landroid/support/v4/graphics/drawable/IconCompat;Ljava/lang/CharSequence;Z)Landroidx/slice/builders/SliceAction;",
			    "(Landroid/app/PendingIntent;Landroidx/core/graphics/drawable/IconCompat;Ljava/lang/CharSequence;Z)Landroidx/slice/builders/SliceAction;"
            )
        ]
	    [InlineData
            (
			    "(Landroid/content/Context;Ljava/io/InputStream;Ljava/lang/String;Landroidx/slice/SliceUtils$SliceActionListener;)Landroidx/slice/Slice;",
			    "(Landroid/content/Context;Ljava/io/InputStream;Ljava/lang/String;Landroidx/slice/SliceUtils$SliceActionistener;)Landroidx/slice/Slice;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/content/Context;Ljava/io/InputStream;Ljava/lang/String;Landroidx/slice/SliceUtils$SliceActionListener;)Landroidx/slice/Slice;",
			    "(Landroid/content/Context;Ljava/io/InputStream;Ljava/lang/String;Landroidx/slice/SliceUtils$SliceActionistener;)Landroidx/slice/Slice;"
            )
        ]
	    [InlineData
            (
			    "(Landroid/content/Context;Ljava/io/InputStream;Landroidx/slice/widget/SliceLiveData$OnErrorListener;)Landroid/arch/lifecycle/LiveData;",
			    "(Landroid/content/Context;Ljava/io/InputStream;Landroidx/slice/widget/SliceiveData$OnErroristener;)Landroidx/lifecycle/LiveData;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/content/Context;Ljava/io/InputStream;Landroidx/slice/widget/SliceLiveData$OnErrorListener;)Landroid/arch/lifecycle/LiveData;",
			    "(Landroid/content/Context;Ljava/io/InputStream;Landroidx/slice/widget/SliceiveData$OnErroristener;)Landroidx/lifecycle/LiveData;"
            )
        ]
	    [InlineData
            (
			    "(Landroid/support/animation/DynamicAnimation$OnAnimationEndListener;)Landroid/support/animation/DynamicAnimation;",
			    "(Landroidx/dynamicanimation/animation/DynamicAnimation$OnAnimationEndistener;)Landroidx/dynamicanimation/animation/DynamicAnimation;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/animation/DynamicAnimation$OnAnimationUpdateListener;)Landroid/support/animation/DynamicAnimation;",
			    "(Landroidx/dynamicanimation/animation/DynamicAnimation$OnAnimationUpdateistener;)Landroidx/dynamicanimation/animation/DynamicAnimation;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/animation/DynamicAnimation$OnAnimationUpdateListener;)Landroid/support/animation/DynamicAnimation;",
			    "(Landroidx/dynamicanimation/animation/DynamicAnimation$OnAnimationUpdateistener;)Landroidx/dynamicanimation/animation/DynamicAnimation;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/animation/DynamicAnimation$OnAnimationEndListener;)V",
			    "(Landroidx/dynamicanimation/animation/DynamicAnimation$OnAnimationEndistener;)V"
            )
        ]
	    [InlineData
            (
			    "(Landroid/content/Context;Landroid/support/v4/provider/FontRequest;)Landroid/support/v4/provider/FontsContractCompat$FontFamilyResult;",
			    "(Landroid/content/Context;Landroidx/core/provider/FontRequest;)Landroidx/core/provider/FontsContractCompat$FontFamilyResult;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/content/Context;Landroid/support/v4/provider/FontRequest;)Landroid/support/v4/provider/FontsContractCompat$FontFamilyResult;",
			    "(Landroid/content/Context;Landroidx/core/provider/FontRequest;)Landroidx/core/provider/FontsContractCompat$FontFamilyResult;"
            )
        ]
	    [InlineData
            (
			    "()Landroid/support/wear/widget/CircularProgressLayout$OnTimerFinishedListener;",
			    "()Landroidx/wear/widget/CircularProgressLayout$OnTimerFinishedListener;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/wear/widget/CircularProgressLayout$OnTimerFinishedListener;)V",
			    "(Landroid/support/wear/widget/CircularProgressayout$OnTimerFinishedistener;)V"
            )
        ]
	    [InlineData
            (
			    "(Landroid/support/wear/widget/ConfirmationOverlay$OnAnimationFinishedListener;)Landroid/support/wear/widget/ConfirmationOverlay;",
			    "(Landroidx/wear/widget/ConfirmationOverlay$OnAnimationFinishedistener;)Landroidx/wear/widget/ConfirmationOverlay;"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/wear/widget/ConfirmationOverlay$OnAnimationFinishedListener;)Landroid/support/wear/widget/ConfirmationOverlay;",
			    "(Landroidx/wear/widget/ConfirmationOverlay$OnAnimationFinishedistener;)Landroidx/wear/widget/ConfirmationOverlay;"
            )
        ]
	    [InlineData
            (
			    "(Landroid/support/wear/widget/drawer/WearableDrawerLayout;Landroid/support/wear/widget/drawer/WearableDrawerView;)V",
			    "(Landroid/support/wear/widget/drawer/WearableDrawerayout;Landroidx/wear/widget/drawer/WearableDrawerView;)V"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/wear/widget/drawer/WearableDrawerLayout;Landroid/support/wear/widget/drawer/WearableDrawerView;)V",
			    "(Landroid/support/wear/widget/drawer/WearableDrawerayout;Landroidx/wear/widget/drawer/WearableDrawerView;)V"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/wear/widget/drawer/WearableDrawerLayout;Landroid/support/wear/widget/drawer/WearableDrawerView;)V",
			    "(Landroid/support/wear/widget/drawer/WearableDrawerayout;Landroidx/wear/widget/drawer/WearableDrawerView;)V"
            )
        ]
	    [InlineData
            (
			    "(Landroid/support/wear/widget/drawer/WearableNavigationDrawerView;Landroid/support/wear/internal/widget/drawer/WearableNavigationDrawerPresenter;)V",
			    "(Landroidx/wear/widget/drawer/WearableNavigationDrawerView;Landroidx/wear/internal/widget/drawer/WearableNavigationDrawerPresenter;)V"
            )
        ]
                        

	    [InlineData
            (
			    "(Landroid/support/wear/widget/drawer/WearableNavigationDrawerView;Landroid/support/wear/internal/widget/drawer/WearableNavigationDrawerPresenter;)V",
			    "(Landroidx/wear/widget/drawer/WearableNavigationDrawerView;Landroidx/wear/internal/widget/drawer/WearableNavigationDrawerPresenter;)V"
            )
        ]
		public void JniStringAreCorrectlyMapped(string supportJni, string androidxJni)
		{
            AndroidXMigrator migrator = null;
            migrator = new AndroidXMigrator(null, null);
            string mapped = migrator.ReplaceJniSignatureRedth(supportJni);

			Assert.Equal(androidxJni, mapped);
		}

	}
}