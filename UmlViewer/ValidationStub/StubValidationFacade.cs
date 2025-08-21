using Validation;

namespace UmlViewer.ValidationStub;

/// <summary>
/// TEMP facade so UmlViewer can run today. Replace with real validation once ready.
/// </summary>
public interface IValidationFacade
{
    // Returns the validated root state (and optionally title).
    (State root, string title) ValidateFromRaw(string rawSpec);
}
