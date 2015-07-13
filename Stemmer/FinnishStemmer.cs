using System;
using System.Linq;
using Annytab.Abstract;

namespace Annytab
{
  /// <summary>
  ///   This class is used to strip finnish words to the steam
  ///   This class is based on the finnish stemming algorithm from Snowball
  ///   http://snowball.tartarus.org/algorithms/finnish/stemmer.html
  /// </summary>
  public class FinnishStemmer : AbstractStemmer
  {
    private readonly string[] _endingsStep1;
    private readonly string[] _endingsStep2;
    private readonly string[] _endingsStep3;
    private readonly string[] _endingsStep4;
    private readonly string[] _longVowels;
    private readonly char[] _restrictedVowels;

    /// <summary>
    ///   Create a new finnish stemmer with default properties
    /// </summary>
    public FinnishStemmer()
      : base()
    {
      // Set values for instance variables
      this.Vowels = new char[] {'a', 'e', 'i', 'o', 'u', 'y', 'ä', 'ö'};
      this._endingsStep1 = new string[] {"kään", "kaan", "hän", "han", "kin", "pä", "pa", "kö", "ko"};
      this._endingsStep2 = new string[] {"nsa", "nsä", "mme", "nne", "si", "ni", "an", "än", "en"};
      this._endingsStep3 = new string[]
                           {
                             "seen", "siin", "tten", "han", "hon", "lle", "ltä", "lta", "llä",
                             "lla", "stä", "hin", "ssä", "ssa", "hen", "hän", "ttä", "tta", "hön", "ksi", "ine", "den",
                             "sta",
                             "nä", "tä", "ta", "na", "a", "ä", "n"
                           };
      this._endingsStep4 = new string[]
                           {
                             "impi", "impä", "immi", "imma", "immä", "impa", "mpi", "eja", "mpa", "mpä", "mmi", "mma",
                             "mmä", "ejä"
                           };
      this._restrictedVowels = new char[] {'a', 'e', 'i', 'o', 'u', 'ä', 'ö'};
      this._longVowels = new string[] {"aa", "ee", "ii", "oo", "uu", "ää", "öö"};
    } // End of the constructor

    /// <summary>
    ///   Get the steam word from a specific word
    /// </summary>
    /// <param name="word">The word to strip</param>
    /// <returns>The stripped word</returns>
    public override string GetSteamWord(string word)
    {
      // Make sure that the word is in lower case characters
      word = word.ToLowerInvariant();

      // Get indexes for R1 and R2
      var partIndexR = CalculateR1R2(word.ToCharArray());

      // Create strings
      var strR1 = partIndexR[0] < word.Length ? word.Substring(partIndexR[0]) : "";

      // **********************************************
      // Step 1
      // **********************************************
      // (a)
      var continueStep1 = true;
      foreach (var end in this._endingsStep1)
      {
        // Check if word ends with some of the predefined step 1 endings
        if (word.EndsWith(end) != true) continue;
        // Get the preceding character
        var precedingChar = word.Length > end.Length ? word[word.Length - end.Length - 1] : '\0';

        // Delete if in R1 and preceded by n, t or a vowel
        if (strR1.EndsWith(end) == true &&
            (precedingChar == 'n' || precedingChar == 't' || IsVowel(precedingChar) == true))
        {
          word = word.Remove(word.Length - end.Length);
          continueStep1 = false;
        }

        // Break out from the loop (the ending has been found)
        break;
      }

      // Recreate strings
      var strR2 = partIndexR[1] < word.Length ? word.Substring(partIndexR[1]) : "";

      // (b)
      if (continueStep1 == true && strR2.EndsWith("sti") == true)
        word = word.Remove(word.Length - 3);
      // **********************************************

      // **********************************************
      // Step 2
      // **********************************************
      strR1 = partIndexR[0] < word.Length ? word.Substring(partIndexR[0]) : "";

      foreach (var end in this._endingsStep2.Where(end => strR1.EndsWith(end) == true))
      {
        switch (end)
        {
          case "nsa":
          case "nsä":
          case "mme":
          case "nne":
            // Delete
            word = word.Remove(word.Length - end.Length);
            break;
          case "si":
            // Delete if not preceded by k
            if (word.EndsWith("ksi") == false)
              word = word.Remove(word.Length - end.Length);
            break;
          case "ni":
            // Delete
            word = word.Remove(word.Length - end.Length);

            // If preceded by kse, replace with ksi
            if (word.EndsWith("kse") == true)
            {
              word = word.Remove(word.Length - 1);
              word += "i";
            }
            break;
          case "an":
            // Delete if preceded by one of:   ta   ssa   sta   lla   lta   na
            if (word.EndsWith("taan") == true || word.EndsWith("ssaan") == true || word.EndsWith("staan") == true ||
                word.EndsWith("llaan") == true
                || word.EndsWith("ltaan") == true || word.EndsWith("naan") == true)
              word = word.Remove(word.Length - end.Length);
            break;
          case "än":
            // Delete if preceded by one of:   tä   ssä   stä   llä   ltä   nä
            if (word.EndsWith("tään") == true || word.EndsWith("ssään") == true || word.EndsWith("stään") == true ||
                word.EndsWith("llään") == true
                || word.EndsWith("ltään") == true || word.EndsWith("nään") == true)
              word = word.Remove(word.Length - end.Length);
            break;
          case "en":
            // Delete if preceded by one of:   lle   ine
            if (word.EndsWith("lleen") == true || word.EndsWith("ineen") == true)
              word = word.Remove(word.Length - end.Length);
            break;
        }

        // Break out from the loop
        break;
      }
      // **********************************************

      // **********************************************
      // Step 3
      // **********************************************
      strR1 = partIndexR[0] < word.Length ? word.Substring(partIndexR[0]) : "";

      var ending_removed_step_3 = false;
      foreach (var end in this._endingsStep3)
      {
        // Check if R1 ends with some of the predefined step 3 endings
        if (strR1.EndsWith(end) == true)
        {
          if (end == "han" || end == "hen" || end == "hin" || end == "hon" || end == "hän" || end == "hön")
          {
            ending_removed_step_3 = true;

            // Get the middle character
            var middleCharacter = end.Substring(1, 1);

            // Delete if preceded by X, where X is a V other than u (a/han, e/hen etc)
            if (word.EndsWith(middleCharacter + end) == true)
            {
              // Delete
              word = word.Remove(word.Length - end.Length);
            }
          }
          else if (end == "siin" == true || end == "tten" == true || end == "den" == true)
          {
            // Get the preceding two letters
            var precedingString = word.Length > (end.Length + 1) ? word.Substring(word.Length - end.Length - 2, 2) : "";

            // Delete if preceded by Vi
            if (this._restrictedVowels.Any(t => precedingString == t.ToString() + "i"))
            {
              word = word.Remove(word.Length - end.Length);
              ending_removed_step_3 = true;
            }
          }
          else
          {
            switch (end)
            {
              case "seen":
                // Get the preceding two letters
                var precedingString = word.Length > (end.Length + 1)
                                        ? word.Substring(word.Length - end.Length - 2, 2)
                                        : "";

                // Delete if preceded by LV
                for (var j = 0; j < this._longVowels.Length; j++)
                {
                  if (precedingString == this._longVowels[j])
                  {
                    word = word.Remove(word.Length - end.Length);
                    ending_removed_step_3 = true;
                    break;
                  }
                }
                break;
              case "a":
              case "ä":
                // Get the preciding two letters
                var before1 = word.Length > 1 ? word[word.Length - 2] : '\0';
                var before2 = word.Length > 2 ? word[word.Length - 3] : '\0';

                // Delete if preceded by cv
                if (word.Length > 2 && IsVowel(before2) == false && IsVowel(before1) == true)
                {
                  word = word.Remove(word.Length - end.Length);
                  ending_removed_step_3 = true;
                }
                break;
              case "tta":
              case "ttä":
                // Delete if preceded by e
                if (word.EndsWith("e" + end) == true)
                {
                  word = word.Remove(word.Length - end.Length);
                  ending_removed_step_3 = true;
                }
                break;
              case "ta":
              case "tä":
              case "ssa":
              case "ssä":
              case "sta":
              case "stä":
              case "lla":
              case "llä":
              case "lta":
              case "ltä":
              case "lle":
              case "na":
              case "nä":
              case "ksi":
              case "ine":
                // Delete
                word = word.Remove(word.Length - end.Length);
                ending_removed_step_3 = true;
                break;
              case "n":
                // Delete
                word = word.Remove(word.Length - end.Length);
                ending_removed_step_3 = true;

                // If preceded by LV or ie, delete the last vowel
                if (word.EndsWith("ie") == true)
                  word = word.Remove(word.Length - 1);
                else
                {
                  // Get the preceding two letters
                  var lastTwoLetters = word.Length > 1 ? word.Substring(word.Length - 2, 2) : "";

                  if (this._longVowels.Any(t => lastTwoLetters == t))
                    word = word.Remove(word.Length - 1);
                }
                break;
            }
          }
        }

        // Break out from the loop if a ending has been removed
        if (ending_removed_step_3 == true)
          break;
      }
      // **********************************************

      // **********************************************
      // Step 4
      // **********************************************
      strR1 = partIndexR[0] < word.Length ? word.Substring(partIndexR[0]) : "";
      strR2 = partIndexR[1] < word.Length ? word.Substring(partIndexR[1]) : "";

      foreach (var end in this._endingsStep4.Where(end => strR2.EndsWith(end) == true))
      {
        switch (end)
        {
          case "mpi":
          case "mpa":
          case "mpä":
          case "mmi":
          case "mma":
          case "mmä":
            // Delete if not preceded by po
            if (word.EndsWith("po" + end) == false)
              word = word.Remove(word.Length - end.Length);
            break;
          case "impi":
          case "impa":
          case "impä":
          case "immi":
          case "imma":
          case "immä":
          case "eja":
          case "ejä":
            // Delete
            word = word.Remove(word.Length - end.Length);
            break;
        }

        // Break out from the loop
        break;
      }
      // **********************************************

      // **********************************************
      // Step 5
      // **********************************************
      strR1 = partIndexR[0] < word.Length ? word.Substring(partIndexR[0]) : "";
      strR2 = partIndexR[1] < word.Length ? word.Substring(partIndexR[1]) : "";

      if (ending_removed_step_3)
      {
        // If an ending was removed in step 3, delete a final i or j if in R1
        if (strR1.EndsWith("i") || strR1.EndsWith("j"))
          word = word.Remove(word.Length - 1);
      }
      else
      {
        // Delete a final t in R1 if it follows a vowel
        if (strR1.EndsWith("t"))
        {
          // Get the preceding char
          var before = word.Length > 1 ? word[word.Length - 2] : '\0';

          if (IsVowel(before))
          {
            word = word.Remove(word.Length - 1);

            strR1 = partIndexR[0] < word.Length ? word.Substring(partIndexR[0]) : "";
            strR2 = partIndexR[1] < word.Length ? word.Substring(partIndexR[1]) : "";

            // If a t is removed, delete a final mma or imma in R2, unless the mma is preceded by po
            if (strR2.EndsWith("imma"))
              word = word.Remove(word.Length - 4);
            else if (strR2.EndsWith("mma") && word.EndsWith("poma") == false)
              word = word.Remove(word.Length - 3);
          }
        }
      }
      // **********************************************

      // **********************************************
      // Step 6
      // **********************************************
      strR1 = partIndexR[0] < word.Length ? word.Substring(partIndexR[0]) : "";
      strR2 = partIndexR[1] < word.Length ? word.Substring(partIndexR[1]) : "";

      // a) If R1 ends LV delete the last letter
      if (this._longVowels.Any(t => strR1.EndsWith(t)))
        word = word.Remove(word.Length - 1);

      strR1 = partIndexR[0] < word.Length ? word.Substring(partIndexR[0]) : "";
      strR2 = partIndexR[1] < word.Length ? word.Substring(partIndexR[1]) : "";

      // b) If R1 ends cX, c a consonant and X one of: a   ä   e   i, delete the last letter
      var c = strR1.Length > 1 ? strR1[strR1.Length - 2] : '\0';
      if (c != '\0' && IsVowel(c) == false &&
          (strR1.EndsWith("a") || strR1.EndsWith("ä") || strR1.EndsWith("e") || strR1.EndsWith("i")))
        word = word.Remove(word.Length - 1);

      strR1 = partIndexR[0] < word.Length ? word.Substring(partIndexR[0]) : "";
      strR2 = partIndexR[1] < word.Length ? word.Substring(partIndexR[1]) : "";

      // c) If R1 ends oj or uj delete the last letter
      if (strR1.EndsWith("oj") || strR1.EndsWith("uj"))
        word = word.Remove(word.Length - 1);

      strR1 = partIndexR[0] < word.Length ? word.Substring(partIndexR[0]) : "";
      strR2 = partIndexR[1] < word.Length ? word.Substring(partIndexR[1]) : "";

      // d) If R1 ends jo delete the last letter
      if (strR1.EndsWith("jo"))
        word = word.Remove(word.Length - 1);

      // e) If the word ends with a double consonant followed by zero or more vowels, remove the last consonant (so eläkk -> eläk, aatonaatto -> aatonaato)
      var startIndex = word.Length - 1;
      for (var i = startIndex; i > -1; i--)
      {
        // Try to find a double consonant
        if (i <= 0 || word[i] != word[i - 1] || IsVowel(word[i]) || IsVowel(word[i - 1])) continue;
        // Get the count of characters that follows the double consonant
        var count = startIndex - i;
        var vowelCount = 0;

        // Count the number of vowels
        for (var j = i; j < word.Length; j++)
        {
          if (IsVowel(word[j]))
            vowelCount += 1;
        }

        // Remove the last consonant
        if (count == vowelCount)
          word = word.Remove(i, 1);

        // Break out from the loop
        break;
      }
      // **********************************************

      // Return the word
      return word.ToLowerInvariant();
    } // End of the GetSteamWord method

    /// <summary>
    ///   Get steam words as a string array from words in a string array
    /// </summary>
    /// <param name="words">An array of words</param>
    /// <returns>An array of steam words</returns>
    public override string[] GetSteamWords(string[] words)
    {
      // Create the string array to return
      var steamWords = new string[words.Length];

      // Loop the list of words
      for (var i = 0; i < words.Length; i++)
        steamWords[i] = GetSteamWord(words[i]);

      // Return the steam word array
      return steamWords;
    } // End of the GetSteamWords method

    /// <summary>
    ///   Calculate indexes for R1 and R2
    /// </summary>
    /// <param name="characters">The char array to calculate indexes for</param>
    /// <returns>An int array with the r1 and r2 index</returns>
    private Int32[] CalculateR1R2(char[] characters)
    {
      // Create ints
      var r1 = characters.Length;
      var r2 = characters.Length;

      // Calculate R1
      for (var i = 1; i < characters.Length; i++)
      {
        if (IsVowel(characters[i]) || IsVowel(characters[i - 1]) != true) continue;
        // Set the r1 index
        r1 = i + 1;
        break;
      }

      // Calculate R2
      for (var i = r1; i < characters.Length; ++i)
      {
        if (IsVowel(characters[i]) || IsVowel(characters[i - 1]) != true) continue;
        // Set the r2 index
        r2 = i + 1;
        break;
      }

      // Return the int array
      return new Int32[] {r1, r2};
    } // End of the CalculateR1R2 method
  } // End of the class
} // End of the namespace