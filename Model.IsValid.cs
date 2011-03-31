using System;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Reflection;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Model.IsValid {

    /// <summary>Extension methods for running DataAnnotation validations on any object</summary>
    public static class ValidationExtensions {

        /// <summary>The validations currently require an instance of a Controller.  Lame, eh?</summary>
        class ConcreteController : Controller {}

        /// <summary>Runs all validations on the given object and returns a ModelStateDictionary</summary>
        public static ModelStateDictionary GetModelState(this object model) {
            var controller = new ConcreteController();
            var context    = new ValidationContext(model, null, null);
            var results    = new List<ValidationResult>();
            var modelState = controller.ModelState;

            // If we use buddy classes, you should associate them with your classes manually before running any of the Model.IsValid validations, eg:
            // TypeDescriptor.AddProviderTransparent(new AssociatedMetadataTypeTypeDescriptionProvider(modelType, buddyType), modelType);

            Validator.TryValidateObject(model, context, results, true);

            foreach (var result in results)
                modelState.AddModelError(result.MemberNames.First(), result.ErrorMessage);

            return modelState;
        }

        /// <summary>Whether or not this object is valid</summary>
        public static bool IsValid(this object model) {
            return model.GetModelState().IsValid;
        }

        /// <summary>Returns a dictionary of all error messages (keyed by property) for the given ModelState</summary>
        public static Dictionary<string, List<string>> ErrorMessagesForAll(this ModelStateDictionary modelState) {
            var messages = new Dictionary<string, List<string>>();
            foreach (var state in modelState) {
                var errorMessages = new List<string>();
                foreach (var error in state.Value.Errors)
                    errorMessages.Add(error.ErrorMessage);
                messages.Add(state.Key, errorMessages);
            }
            return messages;
        }

        /// <summary>Returns a dictionary of all error messages (keyed by property) for the given model</summary>
        public static Dictionary<string, List<string>> ErrorMessagesForAll(this object model) {
            return model.GetModelState().ErrorMessagesForAll();
        }

        /// <summary>Returns a list of all error messages within a ModelState for the given property</summary>
        public static List<string> ErrorMessagesFor(this ModelStateDictionary modelState, string propertyName) {
            try {
                return modelState.ErrorMessagesForAll()[propertyName];
            } catch (KeyNotFoundException) {
                return new List<string>();
            }
        }
        
        /// <summary>Returns a list of all error messages that this model has for the given property</summary>
        public static List<string> ErrorMessagesFor(this object model, string propertyName) {
            try {
                return model.ErrorMessagesForAll()[propertyName];
            } catch (KeyNotFoundException) {
                return new List<string>();
            }
        }

        /// <summary>Returns all of the error message for the provided ModelState (just the messages - not the properties)</summary>
        public static List<string> ErrorMessages(this ModelStateDictionary modelState) {
            var all = new List<string>();
            foreach (var error in modelState.ErrorMessagesForAll())
                all.AddRange(error.Value);
            return all;
        }

        /// <summary>Returns all of the error message for the provided model (just the messages - not the properties)</summary>
        public static List<string> ErrorMessages(this object model) {
            var all = new List<string>();
            foreach (var error in model.ErrorMessagesForAll())
                all.AddRange(error.Value);
            return all;
        }
    }
}
