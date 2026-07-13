import { http } from "@/api/http";
import type { LoginResponse } from "@/api/types";

export interface RegisterInput {
  email: string;
  password: string;
  confirmPassword: string;
  userName: string;
  bio: string;
  birthdate: string;
  countryId: number;
  genderId: number;
  cityId: number;
  /** Chosen profile picture file. */
  image: File | null;
  /** All uploaded files (including the profile picture). */
  newImages: File[];
}

export async function login(email: string, password: string): Promise<LoginResponse> {
  const { data } = await http.post<LoginResponse>("/identity/login", { email, password });
  return data;
}

export async function logout(): Promise<void> {
  await http.post("/identity/logout");
}

export async function refreshToken(): Promise<LoginResponse> {
  const { data } = await http.post<LoginResponse>("/identity/refreshToken");
  return data;
}

// Multipart field names are a backend contract: profile picture under
// "Image", every other file under "NewImages".
export async function register(input: RegisterInput): Promise<void> {
  const form = new FormData();
  form.append("Email", input.email);
  form.append("Password", input.password);
  form.append("ConfirmPassword", input.confirmPassword);
  form.append("UserName", input.userName);
  form.append("Bio", input.bio);
  form.append("Birthdate", input.birthdate);
  form.append("CountryId", String(input.countryId));
  form.append("GenderId", String(input.genderId));
  form.append("CityId", String(input.cityId));

  for (const file of input.newImages) {
    if (input.image && file.name === input.image.name) {
      form.append("Image", file);
    } else {
      form.append("NewImages", file);
    }
  }

  await http.post("/identity/register", form, {
    headers: { "Content-Type": "multipart/form-data" },
  });
}

export async function verifyEmail(token: string, email: string): Promise<string> {
  const { data } = await http.get<string>("/email/verify", { params: { token, email } });
  return data;
}

export async function resendVerificationEmail(email: string): Promise<string> {
  const { data } = await http.get<string>("/email/resendEmailConfirmationLink", { params: { email } });
  return data;
}

export async function sendResetPasswordLink(email: string): Promise<string> {
  const { data } = await http.get<string>("/email/sendResetPasswordLink", { params: { email } });
  return data;
}

export async function resetPassword(input: {
  token: string;
  email: string;
  password: string;
  confirmPassword: string;
}): Promise<string> {
  const { data } = await http.post<string>("/email/resetPassword", input);
  return data;
}
