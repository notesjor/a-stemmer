using System.Linq;

namespace Annytab.Abstract
{
  /// <summary>
  ///   This is the base class for a stemmer
  /// </summary>
  public abstract class AbstractStemmer
  {
    /// <summary>
    ///   Create a new Stemmer
    /// </summary>
    public AbstractStemmer()
    {
      // Set values for instance variables
      Vowels = new char[0];
    } // End of the constructor
    public char[] Vowels { get; protected set; }

    /// <summary>
    ///   Get the steam word from a specific word
    /// </summary>
    /// <param name="word">The word to strip</param>
    /// <returns>The stripped word</returns>
    public abstract string GetSteamWord(string word);

    /// <summary>
    ///   Get steam words as a string array from words in a string array
    /// </summary>
    /// <param name="words">An array of words</param>
    /// <returns>An array of steam words</returns>
    public abstract string[] GetSteamWords(string[] words);

    /// <summary>
    ///   Check if a character is a short syllable
    /// </summary>
    /// <param name="character">The character to check</param>
    /// <returns>A boolean that indicates if the character is a short syllable</returns>
    protected bool IsShortSyllable(char[] characters, int index)
    {
      // Create the boolean to return
      var isShortSyllable = false;

      // Indexes
      var plusOneIndex = index + 1;
      var minusOneIndex = index - 1;

      if (index == 0 && characters.Length > 1)
      {
        if (index == 0 && IsVowel(characters[index]) && IsVowel(characters[plusOneIndex]) == false)
          isShortSyllable = true;
      }
      else if (minusOneIndex > -1 && plusOneIndex < characters.Length)
      {
        if (IsVowel(characters[index]) && IsVowel(characters[plusOneIndex]) == false && characters[plusOneIndex] != 'w' &&
            characters[plusOneIndex] != 'x'
            && characters[plusOneIndex] != 'Y' && IsVowel(characters[minusOneIndex]) == false)
          isShortSyllable = true;
      }

      // Return the boolean
      return isShortSyllable;
    } // End of the IsShortSyllable method

    /// <summary>
    ///   Check if a word is a short word
    /// </summary>
    /// <param name="word">The word to check</param>
    /// <param name="strR1">The r1 string</param>
    /// <returns>A boolean that indicates if the word is a short word</returns>
    protected bool IsShortWord(string word, string strR1)
    {
      return strR1 == "" && IsShortSyllable(word.ToCharArray(), word.Length - 2);
    } // End of the IsShortWord method

    /// <summary>
    ///   Check if a character is a vowel
    /// </summary>
    /// <param name="character">The character to check</param>
    /// <returns>A boolean that indicates if the character is a vowel</returns>
    protected bool IsVowel(char character)
    {
      // Loop the vowel array
      return Vowels.Any(t => character == t);
    } // End of the isVowel method
  } // End of the class
} // End of the namespace