import { instance } from "../services/api";
import { globalConstants } from "../utils/constants";

const baseUrl = globalConstants.API_URL + "email";

export async function verify(token, email) {
  const response = await instance.get(
    `${baseUrl}/verify?token=${token}&email=${email}`
  );

  return response.data;
}

export async function resend(email) {
  const response = await instance.get(
    `${baseUrl}/resendEmailConfirmationLink?email=${email}`
  );

  return response.data;
}
