using System;
using System.Collections.Generic;

namespace HighScore
{
    [Serializable]
    public class HighScoreMap
    {
        [Serializable]
        public class HighScores
        {
            public List<HighScorePair> highScoreList = new ();

            public void AddHighScore(string levelName, float scoreSeconds)
            {
                highScoreList.Add(new HighScorePair(levelName, scoreSeconds));
            }
        }

        [Serializable]
        public class HighScorePair
        {
            public string levelName;
            public float scoreSeconds;

            public HighScorePair(string levelName, float scoreSeconds)
            {
                this.levelName = levelName;
                this.scoreSeconds = scoreSeconds;
            }
        }
    }
}