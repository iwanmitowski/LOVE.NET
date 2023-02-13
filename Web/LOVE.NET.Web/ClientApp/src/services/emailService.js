import { instance } from "../services/api";
import { globalConstants, identityConstants } from "../utils/constants";
import { getErrorMessage } from "../utils/errors"

const baseUrl = globalConstants.API_URL + "email";

export async function verify(token, email) {
  const response = await instance.get(
    `${baseUrl}/verify?token=${token}&email=${email}`
  );

  return response.data;
}

export async function resendVerifyEmail(email) {
  const response = await instance.get(
    `${baseUrl}/resendEmailConfirmationLink?email=${email}`
  );

  return response.data;
}

export async function resetPassword(data) {
  try {
    if (!data.password || !data.confirmPassword) {
      throw new Error(identityConstants.FILL_REQUIRED_FIELDS);
    }
    if (data.confirmPassword !== data.password) {
      throw new Error(identityConstants.PASSWORDS_DONT_MATCH);
    }
    const response = await instance.post(`${baseUrl}/resetPassword`, data);

    return response.data;
  } catch (error) {
    const errorMessage = getErrorMessage(error);
    throw new Error(errorMessage);
  }
}

export async function resendResetPasswordEmail(email) {
  try {
    if (!email) {
      throw new Error(identityConstants.FILL_REQUIRED_FIELDS);
    }
    const response = await instance.get(
      `${baseUrl}/sendResetPasswordLink?email=${email}`
    );

    return response.data;
  } catch (error) {
    const errorMessage = getErrorMessage(error);
    throw new Error(errorMessage);
  }
}
