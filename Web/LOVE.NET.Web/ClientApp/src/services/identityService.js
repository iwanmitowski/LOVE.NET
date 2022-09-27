import axios from "axios";
import { axiosInternal } from "../services/api";
import { globalConstants, identityConstants } from "../utils/constants";

const baseUrl = globalConstants.API_URL + "identity";

export async function login(user) {
  if (!user.email || !user.password) {
    throw new Error(identityConstants.FILL_REQUIRED_FIELDS);
  }

  try {
    const response = await axiosInternal.post(`${baseUrl}/login`, user);
    
    return response.data;
  } catch (error) {
    console.log(error.response.data.error);
    throw new Error(error.response.data.error)
  }
}

export async function logout() {
  try {
    await axiosInternal.post(`${baseUrl}/logout`, {}, {
      headers: {
        withCredentials: true,
        'Authorization': `Bearer eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiI4N2I2MGJlMC1kMmZkLTQxOWUtODgyYy1mNzc2OGM3YzY5ZTMiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJsb3ZlZG90bmV0MkBhYnYuYmciLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoic3RyaW5nIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZWlkZW50aWZpZXIiOiI2YTc2ZDliNC0zNDBjLTRmNzUtYWFlNy05MDMzM2I4Y2I4MjIiLCJleHAiOjE2NjQzMDE0NTYsImlzcyI6Imh0dHBzOi8vbG9jYWxob3N0OjMwMDAvIiwiYXVkIjoiaHR0cHM6Ly9sb2NhbGhvc3Q6MzAwMC8ifQ.nwyymOWUw3Ia_vdPD_pOSobEZzneU8NJChuP9GU7wS-JKHnRWPWb3WdBWLG4KJdfV48iwb_0Q78UmyHX2XFCqQ`
      }
    });
  } catch (error) {
    console.log(error);
  }
}
