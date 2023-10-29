// using Microsoft.AspNetCore.Components;
// using Microsoft.AspNetCore.Components.Forms;

// namespace BlazorApp.Client.Utils;

// public class CustomValidation : ComponentBase
// {
//   private ValidationMessageStore? messageStore;

//   [CascadingParameter]
//   private EditContext? CurrentEditContext { get; set; }

//   protected override void OnInitialized()
//   {
//     if (CurrentEditContext is null)
//     {
//       throw new InvalidOperationException(
//           $"{nameof(CustomValidation)} requires a cascading " +
//           $"parameter of type {nameof(EditContext)}. " +
//           $"For example, you can use {nameof(CustomValidation)} " +
//           $"inside an {nameof(EditForm)}.");
//     }

//     messageStore = new(CurrentEditContext);

//     CurrentEditContext.OnValidationRequested += (s, e) =>
//         messageStore?.Clear();
//     CurrentEditContext.OnFieldChanged += (s, e) =>
//         messageStore?.Clear(e.FieldIdentifier);
//   }

//   public void DisplayErrors(Dictionary<string, List<string>> errors)
//   {
//     if (CurrentEditContext is not null)
//     {
//       foreach (var err in errors)
//       {
//         messageStore?.Add(CurrentEditContext.Field(err.Key), err.Value);
//       }

//       CurrentEditContext.NotifyValidationStateChanged();
//     }
//   }

//   private void ValidateModel(object sender, ValidationRequestedEventArgs e)
//   {
//     // Clear any existing errors
//     messageStore.Clear();

//     // Get the model
//     var model = CurrentEditContext.Model as YourModelType; // Replace YourModelType with the type of your model

//     // Validate the model's properties
//     if (string.IsNullOrEmpty(model.Property1)) // Replace Property1 with the name of a property
//     {
//       messageStore.Add(CurrentEditContext.Field(nameof(model.Property1)), "Property1 is required."); // Replace "Property1 is required." with your error message
//     }

//     // Add more validation rules as needed...

//     // If there are any errors, the form is not valid
//     if (messageStore.())
//     {
//       // The form is not valid, so prevent it from being submitted
//       // However, in Blazor, you don't need to manually prevent the form submission. The form will automatically not be submitted if there are any validation errors.
//     }
//   }

//   public void ClearErrors()
//   {
//     messageStore?.Clear();
//     CurrentEditContext?.NotifyValidationStateChanged();
//   }
// }