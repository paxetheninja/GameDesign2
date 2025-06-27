using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HighScore
{
    public class HighScoreManager : MonoBehaviour
    {
        public static HighScoreManager Instance;
        public string currentLevelName;
    
        private string _persistentPath;
    
        private void Awake()
        {
            if (Instance != null && Instance != this) 
            { 
                Destroy(gameObject); 
            } 
            else 
            { 
                Instance = this;
                DontDestroyOnLoad(gameObject);
                _persistentPath = Application.persistentDataPath + Path.AltDirectorySeparatorChar;
            }
        }

        public static HighScoreMap.HighScores GetHighScoresFromFile()
        {
            string path = Instance._persistentPath + "highscores.json";

            if (!File.Exists(path))
            {
                return new HighScoreMap.HighScores();
            }

            StreamReader reader = new StreamReader(path);
            string json = reader.ReadToEnd();
        
            reader.Close();
            HighScoreMap.HighScores highScores = JsonUtility.FromJson<HighScoreMap.HighScores>(json);
            
            return highScores;
        }

        public static void SaveHighScoreToFile(float scoreSeconds)
        {
            string path = Instance._persistentPath + "highscores.json";

            Debug.Log(path);
        
            var highScores = GetHighScoresFromFile();

            var currentLevelHighScore = highScores.highScoreList.FirstOrDefault(h => h.levelName == Instance.currentLevelName);
            
            if (currentLevelHighScore == null)
            {
                highScores.AddHighScore(Instance.currentLevelName, scoreSeconds);
            }
            else if (currentLevelHighScore.scoreSeconds < scoreSeconds)
            {
                return;
            }
            else
            {
                currentLevelHighScore.scoreSeconds = scoreSeconds;
            }

            using StreamWriter writer = new StreamWriter(path);
            string json = JsonUtility.ToJson(highScores);

            writer.Write(json);
            writer.Close();
        }
    }
}
