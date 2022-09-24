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
