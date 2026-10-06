using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using NTSD.Animation;
using NTSD.App;
using NTSD.UI;
using NTSD.UI.Menu;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace NTSD.Test
{
    public sealed class CharacterSelectionBoardEditorTests
    {
        private const string MenuScene = "Assets/NTSD/Scene/NTSD_Menu.unity";
        private const string BattleScene = "Assets/NTSD/Scene/NTSD_Battle.unity";
        private const string Evidence = "artifacts/diagnostics/NTSD-MENU-CHARACTER-DATA-ITEMS-001/";
        private const string MenuHashKey = "NTSD.CharacterDataItems.MenuHash";
        private const string BattleHashKey = "NTSD.CharacterDataItems.BattleHash";

        [Test]
        public void ChoicesUseRegistryOrderAndDefaultVisibilityWithoutPortraitFiltering()
        {
            var definitions = new List<ObjectDefinition>
            {
                new ObjectDefinition(42, 0, "first"),
                new ObjectDefinition(6, 1, "weapon"),
                new ObjectDefinition(3, 0, "hidden"),
                new ObjectDefinition(17, 0, "second"),
                new ObjectDefinition(42, 0, "duplicate"),
                new ObjectDefinition(99, 0, "missing"),
                new ObjectDefinition(0, 0, "third"),
                null
            };
            var characters = new Dictionary<int, LF2CharacterData>
            {
                [42] = Character(0),
                [3] = Character(1),
                [17] = Character(0),
                [0] = new LF2CharacterData()
            };
            CollectionAssert.AreEqual(new[] { GameConfig.RandomCharacterId, 42, 17, 0 },
                SelectRoleItem.BuildCharacterChoices(definitions,
                    id => characters.TryGetValue(id, out var character) ? character : null));
            CollectionAssert.AreEqual(new[] { GameConfig.RandomCharacterId },
                SelectRoleItem.BuildCharacterChoices(null, null));
            foreach (int hidden in new[] { 1, 2, 3, 4 })
            {
                CollectionAssert.AreEqual(new[] { GameConfig.RandomCharacterId },
                    SelectRoleItem.BuildCharacterChoices(new[] { definitions[0] }, id => Character(hidden)));
            }
            WritePassed("rules", new[] { GameConfig.RandomCharacterId, 42, 17, 0 });
        }

        [Test]
        public void PublishedDataIndexProvidesFiftyVisibleCharactersAfterRandom()
        {
            var source = BattleContentSource.ForLoganRuntime(
                Path.GetFullPath(Path.Combine(Application.dataPath, "NTSD/Content/LoganRuntime")));
            var catalog = LoganObjectCatalog.Read(source, ProjectBattleModeConfig.LoadDefault().Capture());
            var configs = CharacterAnimtorManager.BuildCharacterFrameConfigsFromCatalog(catalog);
            var owner = new GameObject("CharacterDataIndexTest");
            try
            {
                var data = owner.AddComponent<GameDataManager>();
                data.LoadDataFile(source.DataIndexPath);
                var ids = SelectRoleItem.BuildCharacterChoices(data.GetAllObjects(),
                    id => configs.TryGetValue(id, out var config) ? config.characterData : null);
                Assert.AreEqual(51, ids.Count);
                CollectionAssert.AreEqual(new[] { GameConfig.RandomCharacterId, 2, 1, 11 }, ids.Take(4));
                Assert.AreEqual(0, ids.Last());
                var expected = catalog.Entries.Where(entry => entry.Type == 0 &&
                    configs[entry.Id].characterData.NativeMetadata.Bmp.Int32OrDefault("hidden", 0) == 0)
                    .Select(entry => entry.Id);
                CollectionAssert.AreEqual(expected, ids.Skip(1));
                WritePassed("data-index", ids);
            }
            finally
            {
                Object.DestroyImmediate(owner);
            }
        }

        [UnityTest]
        public IEnumerator OriginalMenuGeneratesItemsSelectsRandomAndReusesItemsOnReopen()
        {
            var scene = SceneManager.GetActiveScene();
            if (string.IsNullOrEmpty(scene.path))
            {
                Assert.IsFalse(scene.isDirty);
                Assert.AreEqual(0, scene.rootCount, "Only open Menu from an empty Test Runner scene.");
                EditorSceneManager.OpenScene(MenuScene);
            }
            Assert.AreEqual(MenuScene, SceneManager.GetActiveScene().path);
            Assert.IsFalse(SceneManager.GetActiveScene().isDirty);
            SessionState.SetString(MenuHashKey, Hash(MenuScene));
            SessionState.SetString(BattleHashKey, Hash(BattleScene));
            yield return new EnterPlayMode();
            for (int frame = 0; frame < 12; frame++) yield return null;
            MenuUIController.Instance.ShowLoading();
            float deadline = Time.realtimeSinceStartup + 600f;
            while (CharacterAnimtorManager.TryGetInstance() == null ||
                !CharacterAnimtorManager.TryGetInstance().IsPrewarmCompleted ||
                Object.FindObjectOfType<MenuLoopCarousel>() == null)
            {
                Assert.Less(Time.realtimeSinceStartup, deadline, "Menu character prewarm did not finish.");
                yield return null;
            }
            MenuUIController.Instance.ShowSelectCharacter();
            yield return null;
            var controller = Object.FindObjectOfType<CharacterSelectionController>();
            var board = Object.FindObjectOfType<CharacterSelectionBoard>();
            Assert.IsNotNull(controller);
            Assert.IsNotNull(board);
            var slot = controller.PlayerSlots[0];
            Assert.AreEqual(GameConfig.RandomCharacterId, slot.SelectedCharacterId);
            slot.OnJoin();
            yield return null;
            Assert.AreEqual(SelectRoleState.SelectingCharacter, slot.State);
            Assert.AreEqual(51, board.Choices.Count);
            CollectionAssert.AreEqual(slot.AvailableCharacterIds, board.Choices.Select(cell => cell.CharacterId));
            Assert.AreEqual(GameConfig.RandomCharacterId, board.Choices[0].CharacterId);
            Assert.IsNotNull(board.Choices[0].IconSprite, "Random uses the existing template/config sprite.");
            var resources = CharacterUIResourceManager.TryGetInstance();
            foreach (var cell in board.Choices.Skip(1))
            {
                Assert.IsTrue(cell.gameObject.activeInHierarchy);
                Assert.AreEqual(Vector3.one, cell.transform.localScale);
                var sprites = resources.GetCharacterUISprites(cell.CharacterId);
                Assert.AreSame(sprites.SmallSprite != null ? sprites.SmallSprite : sprites.HeadSprite, cell.IconSprite);
            }
            var instances = board.Choices.Select(cell => cell.GetInstanceID()).ToArray();
            var first = board.Choices[1];
            var click = new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left };
            first.OnPointerClick(click);
            Assert.AreEqual(first.CharacterId, slot.SelectedCharacterId);
            slot.RefreshAvailableCharacters();
            Assert.AreEqual(first.CharacterId, slot.SelectedCharacterId, "Refreshing keeps a valid concrete choice.");
            first.OnPointerClick(click);
            Assert.AreEqual(SelectRoleState.SelectingTeam, slot.State);
            slot.OnCancel();
            Assert.AreEqual(SelectRoleState.SelectingCharacter, slot.State);

            Directory.CreateDirectory(Evidence);
            string screenshot = Evidence + "character-items-" + System.DateTime.UtcNow.ToString("yyyyMMdd-HHmmssfff") + ".png";
            ScreenCapture.CaptureScreenshot(screenshot);
            for (int frame = 0; frame < 120 && !File.Exists(screenshot); frame++) yield return null;
            Assert.IsTrue(File.Exists(screenshot));
            MenuUIController.Instance.ShowMainMenu();
            yield return null;
            Assert.AreEqual(0, board.Choices.Count);
            MenuUIController.Instance.ShowSelectCharacter();
            yield return null;
            slot.OnJoin();
            yield return null;
            Assert.AreEqual(GameConfig.RandomCharacterId, slot.SelectedCharacterId);
            Assert.AreEqual(51, board.Choices.Count);
            CollectionAssert.AreEqual(instances, board.Choices.Select(cell => cell.GetInstanceID()));
            int[] finalIds = board.Choices.Select(cell => cell.CharacterId).ToArray();
            yield return new ExitPlayMode();
            Assert.AreEqual(SessionState.GetString(MenuHashKey, ""), Hash(MenuScene));
            Assert.AreEqual(SessionState.GetString(BattleHashKey, ""), Hash(BattleScene));
            Assert.IsFalse(SceneManager.GetActiveScene().isDirty);
            WritePassed("original-menu-reopen", finalIds);
        }

        private static LF2CharacterData Character(int hidden)
        {
            return new LF2CharacterData
            {
                NativeMetadata = new LoganDefinitionMetadata(
                    new LoganDefinitionFieldSet(new[] { new KeyValuePair<string, string>("hidden", hidden.ToString()) }),
                    new LoganDefinitionFieldSet(new KeyValuePair<string, string>[0]))
            };
        }

        private static string Hash(string path)
        {
            using (var stream = File.OpenRead(path))
            using (var sha = SHA256.Create())
                return System.BitConverter.ToString(sha.ComputeHash(stream));
        }

        [System.Serializable]
        private sealed class PassedEvidence
        {
            public string check;
            public int[] characterIds;
            public string menuSha256;
            public string battleSha256;
        }

        private static void WritePassed(string check, IEnumerable<int> ids)
        {
            Directory.CreateDirectory(Evidence);
            var result = new PassedEvidence
            {
                check = check,
                characterIds = ids.ToArray(),
                menuSha256 = Hash(MenuScene),
                battleSha256 = Hash(BattleScene)
            };
            File.WriteAllText(Evidence + "passed-" + check + "-" +
                System.DateTime.UtcNow.ToString("yyyyMMdd-HHmmssfff") + ".json", JsonUtility.ToJson(result, true));
        }

        [UnityTearDown]
        public IEnumerator ExitTestPlay()
        {
            if (Application.isPlaying) yield return new ExitPlayMode();
        }
    }
}
