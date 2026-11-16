using System;
using System.IO;
using System.Linq;
using MobileGame.Presentation;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;
namespace MobileGame.Foundation.Editor
{
    // Approved handoff only. Re-running preserves asset GUIDs and Sprite IDs.
    public static class BatchArtAssets
    {
        public const string SkinPath = "Assets/SO/UI/UiSkin.asset";
        private sealed class Entry
        {
            public string Id, Source, Receive, Final; public int Width, Height, Columns, SheetHeight, Border; public string[] Names;
            public Entry(string id,string source,string receive,string final,int width,int height,int columns,int sheetHeight,int border,string[] names)
            { Id=id;Source=source;Receive=receive;Final=final;Width=width;Height=height;Columns=columns;SheetHeight=sheetHeight;Border=border;Names=names; }
        }
        private static readonly Entry[] Entries = {
            new Entry("Invader", "C:/Dev/Unity/Art/assets/characters/Invader/revisions/r002/export/Invader.png", "Assets/Sprites/Require/PLAN-034/PLAN-034-SPR-001/r002/Invader.png", "Assets/Sprites/Creature/Invader/Invader.png", 64, 64, 10, 256, 0, new[] { "Invader_Idle_Down_01", "Invader_Idle_Down_02", "Invader_Idle_Down_03", "Invader_Idle_Down_04", "Invader_Move_Down_01", "Invader_Move_Down_02", "Invader_Move_Down_03", "Invader_Move_Down_04", "Invader_Move_Down_05", "Invader_Move_Down_06", "Invader_Idle_Left_01", "Invader_Idle_Left_02", "Invader_Idle_Left_03", "Invader_Idle_Left_04", "Invader_Move_Left_01", "Invader_Move_Left_02", "Invader_Move_Left_03", "Invader_Move_Left_04", "Invader_Move_Left_05", "Invader_Move_Left_06", "Invader_Idle_Right_01", "Invader_Idle_Right_02", "Invader_Idle_Right_03", "Invader_Idle_Right_04", "Invader_Move_Right_01", "Invader_Move_Right_02", "Invader_Move_Right_03", "Invader_Move_Right_04", "Invader_Move_Right_05", "Invader_Move_Right_06", "Invader_Idle_Up_01", "Invader_Idle_Up_02", "Invader_Idle_Up_03", "Invader_Idle_Up_04", "Invader_Move_Up_01", "Invader_Move_Up_02", "Invader_Move_Up_03", "Invader_Move_Up_04", "Invader_Move_Up_05", "Invader_Move_Up_06" }),
            new Entry("Raider", "C:/Dev/Unity/Art/assets/characters/Raider/revisions/r002/export/Raider.png", "Assets/Sprites/Require/PLAN-034/PLAN-034-SPR-002/r002/Raider.png", "Assets/Sprites/Creature/Raider/Raider.png", 64, 64, 10, 256, 0, new[] { "Raider_Idle_Down_01", "Raider_Idle_Down_02", "Raider_Idle_Down_03", "Raider_Idle_Down_04", "Raider_Move_Down_01", "Raider_Move_Down_02", "Raider_Move_Down_03", "Raider_Move_Down_04", "Raider_Move_Down_05", "Raider_Move_Down_06", "Raider_Idle_Left_01", "Raider_Idle_Left_02", "Raider_Idle_Left_03", "Raider_Idle_Left_04", "Raider_Move_Left_01", "Raider_Move_Left_02", "Raider_Move_Left_03", "Raider_Move_Left_04", "Raider_Move_Left_05", "Raider_Move_Left_06", "Raider_Idle_Right_01", "Raider_Idle_Right_02", "Raider_Idle_Right_03", "Raider_Idle_Right_04", "Raider_Move_Right_01", "Raider_Move_Right_02", "Raider_Move_Right_03", "Raider_Move_Right_04", "Raider_Move_Right_05", "Raider_Move_Right_06", "Raider_Idle_Up_01", "Raider_Idle_Up_02", "Raider_Idle_Up_03", "Raider_Idle_Up_04", "Raider_Move_Up_01", "Raider_Move_Up_02", "Raider_Move_Up_03", "Raider_Move_Up_04", "Raider_Move_Up_05", "Raider_Move_Up_06" }),
            new Entry("LaboratoryBoundary", "C:/Dev/Unity/Art/assets/backgrounds/LaboratoryBoundary/revisions/r001/export/LaboratoryBoundary.png", "Assets/Sprites/Require/PLAN-036/PLAN-036-SPR-001/r001/LaboratoryBoundary.png", "Assets/Sprites/World/LaboratoryBoundary/LaboratoryBoundary.png", 64, 64, 4, 128, 0, new[] { "LaboratoryBoundary_Top", "LaboratoryBoundary_Right", "LaboratoryBoundary_Bottom", "LaboratoryBoundary_Left", "LaboratoryBoundary_TopLeft", "LaboratoryBoundary_TopRight", "LaboratoryBoundary_BottomRight", "LaboratoryBoundary_BottomLeft" }),
            new Entry("ZoomOut", "C:/Dev/Unity/Art/assets/ui/ZoomOut/revisions/r001/export/ZoomOut.png", "Assets/Sprites/Require/PLAN-014/PLAN-014-SPR-001/r001/ZoomOut.png", "Assets/Sprites/UI/Laboratory/ZoomOut.png", 64, 64, 1, 64, 0, new[] { "ZoomOut" }),
            new Entry("ZoomIn", "C:/Dev/Unity/Art/assets/ui/ZoomIn/revisions/r001/export/ZoomIn.png", "Assets/Sprites/Require/PLAN-014/PLAN-014-SPR-002/r001/ZoomIn.png", "Assets/Sprites/UI/Laboratory/ZoomIn.png", 64, 64, 1, 64, 0, new[] { "ZoomIn" }),
            new Entry("FitMap", "C:/Dev/Unity/Art/assets/ui/FitMap/revisions/r001/export/FitMap.png", "Assets/Sprites/Require/PLAN-014/PLAN-014-SPR-003/r001/FitMap.png", "Assets/Sprites/UI/Laboratory/FitMap.png", 64, 64, 1, 64, 0, new[] { "FitMap" }),
            new Entry("Creatures", "C:/Dev/Unity/Art/assets/ui/Creatures/revisions/r001/export/Creatures.png", "Assets/Sprites/Require/PLAN-014/PLAN-014-SPR-007/r001/Creatures.png", "Assets/Sprites/UI/Laboratory/Creatures.png", 64, 64, 1, 64, 0, new[] { "Creatures" }),
            new Entry("Invasion", "C:/Dev/Unity/Art/assets/ui/Invasion/revisions/r001/export/Invasion.png", "Assets/Sprites/Require/PLAN-014/PLAN-014-SPR-008/r001/Invasion.png", "Assets/Sprites/UI/Laboratory/Invasion.png", 64, 64, 1, 64, 0, new[] { "Invasion" }),
            new Entry("More", "C:/Dev/Unity/Art/assets/ui/More/revisions/r001/export/More.png", "Assets/Sprites/Require/PLAN-014/PLAN-014-SPR-009/r001/More.png", "Assets/Sprites/UI/Laboratory/More.png", 64, 64, 1, 64, 0, new[] { "More" }),
            new Entry("PanelFrame", "C:/Dev/Unity/Art/assets/ui/PanelFrame/revisions/r001/export/PanelFrame.png", "Assets/Sprites/Require/PLAN-038/PLAN-038-SPR-001/r001/PanelFrame.png", "Assets/Sprites/UI/Frames/PanelFrame.png", 32, 32, 1, 32, 4, new[] { "PanelFrame" }),
            new Entry("ButtonFrame", "C:/Dev/Unity/Art/assets/ui/ButtonFrame/revisions/r001/export/ButtonFrame.png", "Assets/Sprites/Require/PLAN-038/PLAN-038-SPR-002/r001/ButtonFrame.png", "Assets/Sprites/UI/Frames/ButtonFrame.png", 32, 32, 1, 32, 3, new[] { "ButtonFrame" }),
            new Entry("SelectionFrame", "C:/Dev/Unity/Art/assets/ui/SelectionFrame/revisions/r001/export/SelectionFrame.png", "Assets/Sprites/Require/PLAN-038/PLAN-038-SPR-003/r001/SelectionFrame.png", "Assets/Sprites/UI/Frames/SelectionFrame.png", 32, 32, 1, 32, 3, new[] { "SelectionFrame" }),
            new Entry("ScrollThumb", "C:/Dev/Unity/Art/assets/ui/ScrollThumb/revisions/r001/export/ScrollThumb.png", "Assets/Sprites/Require/PLAN-038/PLAN-038-SPR-004/r001/ScrollThumb.png", "Assets/Sprites/UI/Frames/ScrollThumb.png", 32, 32, 1, 32, 3, new[] { "ScrollThumb" }),
            new Entry("HudFrame", "C:/Dev/Unity/Art/assets/ui/HudFrame/revisions/r001/export/HudFrame.png", "Assets/Sprites/Require/PLAN-038/PLAN-038-SPR-005/r001/HudFrame.png", "Assets/Sprites/UI/Frames/HudFrame.png", 32, 32, 1, 32, 4, new[] { "HudFrame" }),
            new Entry("DestructionTrack", "C:/Dev/Unity/Art/assets/ui/DestructionTrack/revisions/r001/export/DestructionTrack.png", "Assets/Sprites/Require/PLAN-038/PLAN-038-SPR-006/r001/DestructionTrack.png", "Assets/Sprites/UI/Frames/DestructionTrack.png", 16, 16, 1, 16, 1, new[] { "DestructionTrack" }),
            new Entry("BirthPulse", "C:/Dev/Unity/Art/assets/vfx/BirthPulse/revisions/r001/export/BirthPulse.png", "Assets/Sprites/Require/PLAN-039/PLAN-039-SPR-001/r001/BirthPulse.png", "Assets/Sprites/VFX/Birth/BirthPulse.png", 32, 32, 1, 32, 0, new[] { "BirthPulse" }),
            new Entry("ReleaseTrail", "C:/Dev/Unity/Art/assets/vfx/ReleaseTrail/revisions/r001/export/ReleaseTrail.png", "Assets/Sprites/Require/PLAN-039/PLAN-039-SPR-002/r001/ReleaseTrail.png", "Assets/Sprites/VFX/Release/ReleaseTrail.png", 32, 32, 1, 32, 0, new[] { "ReleaseTrail" }),
            new Entry("TitleLandscape", "C:/Dev/Unity/Art/assets/ui/TitleLandscape/revisions/r002/export/TitleLandscape.png", "Assets/Sprites/Require/PLAN-040/PLAN-040-SPR-001/r002/TitleLandscape.png", "Assets/Sprites/UI/Title/TitleLandscape.png", 640, 360, 4, 1440, 0, new[] { "TitleLandscape_Ambient_01", "TitleLandscape_Ambient_02", "TitleLandscape_Ambient_03", "TitleLandscape_Ambient_04", "TitleLandscape_Ambient_05", "TitleLandscape_Ambient_06", "TitleLandscape_Ambient_07", "TitleLandscape_Ambient_08", "TitleLandscape_Ambient_09", "TitleLandscape_Ambient_10", "TitleLandscape_Ambient_11", "TitleLandscape_Ambient_12", "TitleLandscape_Ambient_13", "TitleLandscape_Ambient_14", "TitleLandscape_Ambient_15", "TitleLandscape_Ambient_16" }),
            new Entry("TitlePortrait", "C:/Dev/Unity/Art/assets/ui/TitlePortrait/revisions/r002/export/TitlePortrait.png", "Assets/Sprites/Require/PLAN-040/PLAN-040-SPR-002/r002/TitlePortrait.png", "Assets/Sprites/UI/Title/TitlePortrait.png", 360, 640, 4, 2560, 0, new[] { "TitlePortrait_Ambient_01", "TitlePortrait_Ambient_02", "TitlePortrait_Ambient_03", "TitlePortrait_Ambient_04", "TitlePortrait_Ambient_05", "TitlePortrait_Ambient_06", "TitlePortrait_Ambient_07", "TitlePortrait_Ambient_08", "TitlePortrait_Ambient_09", "TitlePortrait_Ambient_10", "TitlePortrait_Ambient_11", "TitlePortrait_Ambient_12", "TitlePortrait_Ambient_13", "TitlePortrait_Ambient_14", "TitlePortrait_Ambient_15", "TitlePortrait_Ambient_16" }),
            new Entry("LoadingDecoration", "C:/Dev/Unity/Art/assets/ui/LoadingDecoration/revisions/r002/export/LoadingDecoration.png", "Assets/Sprites/Require/PLAN-040/PLAN-040-SPR-003/r002/LoadingDecoration.png", "Assets/Sprites/UI/Loading/LoadingDecoration.png", 64, 64, 4, 256, 0, new[] { "LoadingDecoration_Bubbles_01", "LoadingDecoration_Bubbles_02", "LoadingDecoration_Bubbles_03", "LoadingDecoration_Bubbles_04", "LoadingDecoration_Bubbles_05", "LoadingDecoration_Bubbles_06", "LoadingDecoration_Bubbles_07", "LoadingDecoration_Bubbles_08", "LoadingDecoration_Bubbles_09", "LoadingDecoration_Bubbles_10", "LoadingDecoration_Bubbles_11", "LoadingDecoration_Bubbles_12", "LoadingDecoration_Bubbles_13", "LoadingDecoration_Bubbles_14", "LoadingDecoration_Bubbles_15", "LoadingDecoration_Bubbles_16" }),
            new Entry("WorldDestruction", "C:/Dev/Unity/Art/assets/ui/WorldDestruction/revisions/r002/export/WorldDestruction.png", "Assets/Sprites/Require/PLAN-041/PLAN-041-SPR-001/r002/WorldDestruction.png", "Assets/Sprites/UI/Ending/WorldDestruction.png", 128, 128, 8, 512, 0, new[] { "WorldDestruction_Colonization_01", "WorldDestruction_Colonization_02", "WorldDestruction_Colonization_03", "WorldDestruction_Colonization_04", "WorldDestruction_Colonization_05", "WorldDestruction_Colonization_06", "WorldDestruction_Colonization_07", "WorldDestruction_Colonization_08", "WorldDestruction_Colonization_09", "WorldDestruction_Colonization_10", "WorldDestruction_Colonization_11", "WorldDestruction_Colonization_12", "WorldDestruction_Colonization_13", "WorldDestruction_Colonization_14", "WorldDestruction_Colonization_15", "WorldDestruction_Colonization_16", "WorldDestruction_Colonization_17", "WorldDestruction_Colonization_18", "WorldDestruction_Colonization_19", "WorldDestruction_Colonization_20", "WorldDestruction_Colonization_21", "WorldDestruction_Colonization_22", "WorldDestruction_Colonization_23", "WorldDestruction_Colonization_24", "WorldDestruction_Colonization_25", "WorldDestruction_Colonization_26", "WorldDestruction_Colonization_27", "WorldDestruction_Colonization_28", "WorldDestruction_Colonization_29", "WorldDestruction_Colonization_30", "WorldDestruction_Colonization_31", "WorldDestruction_Colonization_32" })
        };
        public static void Install()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Install artwork outside Play Mode.");
            foreach (var entry in Entries)
            {
                FoundationDefinitionAssets.EnsureFolder(Path.GetDirectoryName(entry.Final).Replace('\\','/'));
                if (!File.Exists(entry.Final))
                {
                    FoundationDefinitionAssets.EnsureFolder(Path.GetDirectoryName(entry.Receive).Replace('\\','/'));
                    File.Copy(entry.Source,entry.Receive,true);
                    AssetDatabase.ImportAsset(entry.Receive,ImportAssetOptions.ForceSynchronousImport);
                    var error=AssetDatabase.MoveAsset(entry.Receive,entry.Final);
                    if (!string.IsNullOrEmpty(error)) throw new IOException(error);
                }
                Import(entry);
            }
            EnsureReferences();
        }
        private static void Import(Entry entry)
        {
            var importer=(TextureImporter)AssetImporter.GetAtPath(entry.Final);
            importer.textureType=TextureImporterType.Sprite;
            importer.spriteImportMode=entry.Names.Length==1?SpriteImportMode.Single:SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit=32; importer.filterMode=FilterMode.Point; importer.wrapMode=TextureWrapMode.Clamp;
            importer.textureCompression=TextureImporterCompression.Uncompressed;importer.mipmapEnabled=false;importer.isReadable=false;
            importer.sRGBTexture=true;importer.alphaIsTransparency=true;importer.alphaSource=TextureImporterAlphaSource.FromInput;
            importer.npotScale=TextureImporterNPOTScale.None;importer.maxTextureSize=4096;
            foreach(var platform in new[]{"Android","Standalone","iPhone","WebGL"}) importer.ClearPlatformTextureSettings(platform);
            var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);
            settings.spriteMeshType=SpriteMeshType.FullRect;settings.spriteAlignment=(int)SpriteAlignment.Center;
            settings.spritePivot=Vector2.one*.5f;settings.spriteBorder=Vector4.one*entry.Border;importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
            if(entry.Names.Length==1) return;
            var factory=new SpriteDataProviderFactories();factory.Init();
            var provider=factory.GetSpriteEditorDataProviderFromObject(importer);provider.InitSpriteEditorDataProvider();
            var edit=provider.GetDataProvider<ISpriteFrameEditCapability>();
            if(edit==null) throw new InvalidOperationException("Sprite capability missing: "+entry.Final);
            foreach(var capability in new[]{EEditCapability.CreateAndDeleteSprite,EEditCapability.EditSpriteName,EEditCapability.EditSpriteRect,EEditCapability.EditPivot,EEditCapability.EditBorder})
                if(!edit.GetEditCapability().HasCapability(capability)) throw new InvalidOperationException("Unsupported sprite edit: "+capability);
            var old=provider.GetSpriteRects();
            var rects=entry.Names.Select((name,i)=>new SpriteRect { name=name,
                rect=new Rect(i%entry.Columns*entry.Width,entry.SheetHeight-(i/entry.Columns+1)*entry.Height,entry.Width,entry.Height),
                alignment=SpriteAlignment.Custom, pivot=new Vector2(.5f,entry.Id=="Invader"||entry.Id=="Raider"?.15625f:.5f),
                border=Vector4.one*entry.Border,spriteID=old.FirstOrDefault(r=>r.name==name)?.spriteID??GUID.Generate() }).ToArray();
            provider.SetSpriteRects(rects);
            var names=provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
            if(names==null) throw new InvalidOperationException("Sprite file ID provider missing.");
            names.SetNameFileIdPairs(rects.Select(r=>new SpriteNameFileIdPair(r.name,r.spriteID)));
            provider.Apply();importer.SaveAndReimport();
        }
        public static Sprite[] Frames(string id)
        {
            var entry=Entries.Single(e=>e.Id==id);
            var sprites=AssetDatabase.LoadAllAssetsAtPath(entry.Final).OfType<Sprite>().ToArray();
            return entry.Names.Select(n=>sprites.SingleOrDefault(s=>s.name==n)).ToArray();
        }
        public static bool Ready => File.Exists(Entries.Last().Final);
        private static void SetFrames(SerializedProperty property,Sprite[] frames)
        { property.arraySize=frames.Length;for(var i=0;i<frames.Length;i++) property.GetArrayElementAtIndex(i).objectReferenceValue=frames[i]; }
        public static void EnsureReferences()
        {
            if(!Ready) return;
            FoundationDefinitionAssets.EnsureFolder("Assets/SO/UI");
            var skin=AssetDatabase.LoadAssetAtPath<UiSkinAssets>(SkinPath);
            if(skin==null){skin=ScriptableObject.CreateInstance<UiSkinAssets>();AssetDatabase.CreateAsset(skin,SkinPath);}
            skin.panel=Frames("PanelFrame")[0];skin.button=Frames("ButtonFrame")[0];skin.selection=Frames("SelectionFrame")[0];
            skin.scrollThumb=Frames("ScrollThumb")[0];skin.hud=Frames("HudFrame")[0];skin.destructionTrack=Frames("DestructionTrack")[0];
            EditorUtility.SetDirty(skin);AssetDatabase.SaveAssetIfDirty(skin);
            var art=AssetDatabase.LoadAssetAtPath<LaboratoryArtAssets>(LaboratoryUiAssetBuilder.ArtPath);
            using(var data=new SerializedObject(art))
            {
                SetFrames(data.FindProperty("boundary"),Frames("LaboratoryBoundary"));
                SetFrames(data.FindProperty("colonization"),Frames("WorldDestruction"));
                data.FindProperty("birthPulse").objectReferenceValue=Frames("BirthPulse")[0];
                data.FindProperty("releaseTrail").objectReferenceValue=Frames("ReleaseTrail")[0];
                foreach(var id in new[]{"Invader","Raider"})
                {
                    var definition=AssetDatabase.LoadAssetAtPath<CreatureDefinition>("Assets/SO/Creature/"+id+".asset");
                    var entries=data.FindProperty("creatures");var index=0;
                    while(index<entries.arraySize&&entries.GetArrayElementAtIndex(index).FindPropertyRelative("definition").objectReferenceValue!=definition) index++;
                    if(index==entries.arraySize) entries.arraySize++;
                    var item=entries.GetArrayElementAtIndex(index);item.FindPropertyRelative("definition").objectReferenceValue=definition;
                    SetFrames(item.FindPropertyRelative("frames"),Frames(id));item.FindPropertyRelative("heads").arraySize=0;
                    item.FindPropertyRelative("framesPerDirection").intValue=10;
                    item.FindPropertyRelative("idleSeconds").floatValue=id=="Invader"?.25f:.2f;
                    item.FindPropertyRelative("moveSeconds").floatValue=id=="Invader"?.12f:.1f;
                    using var binding=new SerializedObject(definition);binding.FindProperty("icon").objectReferenceValue=Frames(id)[0];
                    binding.ApplyModifiedPropertiesWithoutUndo();AssetDatabase.SaveAssetIfDirty(definition);
                }
                data.ApplyModifiedPropertiesWithoutUndo();AssetDatabase.SaveAssetIfDirty(art);
            }
        }
        public static void BindOwner(UnityEngine.Object owner)
        {
            if(!Ready) return;
            using var data=new SerializedObject(owner);
            var ui=data.FindProperty("uiAssets");
            if(ui!=null)
            {
                ui.FindPropertyRelative("skin").objectReferenceValue=AssetDatabase.LoadAssetAtPath<UiSkinAssets>(SkinPath);
                var icons=ui.FindPropertyRelative("icons");
                foreach(var id in new[]{"ZoomOut","ZoomIn","FitMap","Creatures","Invasion","More"})
                    icons.GetArrayElementAtIndex((int)Enum.Parse(typeof(LaboratoryIcon),id)).objectReferenceValue=Frames(id)[0];
            }
            if(owner is TitleView)
            {
                SetFrames(data.FindProperty("landscapeFrames"),Frames("TitleLandscape"));SetFrames(data.FindProperty("portraitFrames"),Frames("TitlePortrait"));
                data.FindProperty("landscapeRoom").objectReferenceValue=Frames("TitleLandscape")[0];data.FindProperty("portraitRoom").objectReferenceValue=Frames("TitlePortrait")[0];
                data.FindProperty("wordmark").objectReferenceValue=null;data.FindProperty("buttonFrame").objectReferenceValue=Frames("ButtonFrame")[0];
            }
            if(owner is LoadingScreen)
            {
                SetFrames(data.FindProperty("decorationFrames"),Frames("LoadingDecoration"));
                data.FindProperty("skin").objectReferenceValue=AssetDatabase.LoadAssetAtPath<UiSkinAssets>(SkinPath);
            }
            data.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(owner);
        }
    }
}
