using UnityEngine;

namespace GGemCo.Scripts
{
    public static class DialogueCharacterHelper
    {
        public static string GetName(DialogueNodeData dialogue)
        {
            if (dialogue.characterType == CharacterConstants.Type.Npc)
            {
                var data = TableLoaderManager.Instance.TableNpc.GetDataByUid(dialogue.characterUid);
                return data?.Name ?? "???";
            }
            return string.Empty;
        }

        public static Sprite GetThumbnail(DialogueNodeData dialogue)
        {
            if (dialogue == null) return null;
            if (dialogue.thumbnailImage != "")
            {
                return Resources.Load<Sprite>($"Images/Thumbnail/{dialogue.thumbnailImage}");
            }
            if (dialogue.characterType == CharacterConstants.Type.Npc)
            {
                var data = TableLoaderManager.Instance.TableNpc.GetDataByUid(dialogue.characterUid);
                if (data != null)
                    return Resources.Load<Sprite>($"Images/Thumbnail/Npc/{data.ImageThumbnailPath}");
            }
            return null;
        }
    }
}