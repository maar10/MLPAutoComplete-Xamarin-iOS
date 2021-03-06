﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;

namespace MLPAutoComplete
{
	public class MLPAutoCompleteSortOperation
	{
		private MLPAutoCompleteTextField _textField;

		public MLPAutoCompleteSortOperation (MLPAutoCompleteTextField textField)
		{
			_textField = textField;
		}

		public List<Object> Sort(string inputString, List<Object> completions)
		{
			if (String.IsNullOrEmpty (inputString)) { 
				return completions;
			}

			List<Dictionary<int,Object>> editDistances = new List<Dictionary<int,Object>> ();

			foreach (var completion in completions) {

				string currentString = String.Empty;
				if (completion is string) {
					currentString = (string)completion;
				} else if (completion is MLPAutoCompletionObject) {
					currentString = ((MLPAutoCompletionObject)completion).AutocompleteString;
				} else {
					Debug.WriteLine ("Autocompletion terms must either be strings or objects conforming to the MLPAutoCompleteObject protocol.");
				}

				int maxRange = (inputString.Length < currentString.Length) ? inputString.Length : currentString.Length;
				int editDistanceOfCurrentString = LevenshteinDistance.Compute (inputString, currentString);

				if (editDistanceOfCurrentString > maxRange) { 
					continue;
				}

				Dictionary<int,Object> stringsWithEditDistances = new Dictionary<int,Object>  ();
				stringsWithEditDistances.Add(1, currentString);
				stringsWithEditDistances.Add(2, completion);
				stringsWithEditDistances.Add(3, editDistanceOfCurrentString);

				editDistances.Add (stringsWithEditDistances);
			}
				
			editDistances.Sort ((Dictionary<int, object> x, Dictionary<int, object> y) => {
				var first = (int) x[3];
				var second = (int) y[3];
				return first.CompareTo(second);
			});

			List<Object> otherSuggestions = new List<Object> ();
			List<Object> prioritySuggestions = new List<Object> ();

			foreach(var stringWithEditDistances in editDistances)
			{
				Object autoCompleteObject = stringWithEditDistances [1];
				string suggestedString = (string)stringWithEditDistances [2];

				var suggestedStringComponents = suggestedString.Split (null);
				bool suggestedStringDeservesPriority = false;

				foreach (var component in suggestedStringComponents) {

					if(inputString.Length != 0 && (component.ToLower ().IndexOf(inputString.ToLower ()) == 0)){
						suggestedStringDeservesPriority = true;
						prioritySuggestions.Add (autoCompleteObject);
						break;
					}

					if (inputString.Length <= 1)
						break;
				}

				if (!suggestedStringDeservesPriority)
					otherSuggestions.Add (autoCompleteObject);

			}

			var result = new List<Object>();
			result.AddRange (prioritySuggestions);
			result.AddRange (otherSuggestions);
			return result;
		}
	}
}
