import { useState } from "react";

export const useLocalStorage = (key, defaultValue) => {
  const [value, setValue] = useState(() => {
    const storedValue = localStorage.getItem(key);

    return !!storedValue ? JSON.parse(storedValue) : defaultValue;
  });

  const setLocalStorageValue = (newValue) => {
    localStorage.setItem(key, JSON.stringify(newValue));

    if(!!newValue) {
      setValue(newValue);
    }
    else {
      localStorage.removeItem(key);
    }
  };

  return [value, setLocalStorageValue];
};