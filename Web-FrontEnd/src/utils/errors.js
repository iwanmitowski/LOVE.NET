export function getErrorMessage(error) {
    // Client side validation error
    const validationError = error?.response?.data?.Error;
    // Serverside validation error
    const validationErrors =
      (error?.response?.data?.errors &&
        Object.values(error?.response?.data?.errors)) ||
      [];
  
    let errors = [...validationErrors, validationError].filter((e) => !!e);
    if (!errors.length) {
      errors = [...errors, error.message, ...(error?.response?.data || [])];
    }
  
    const errorMessage = errors.join("\n");
  
    return errorMessage;
  }