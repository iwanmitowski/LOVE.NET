import { http } from "@/api/http";
import type { AccountDetails, ImageModel } from "@/api/types";

export async function getAccount(id: string): Promise<AccountDetails> {
  const { data } = await http.get<AccountDetails>(`/profile/account/${id}`);
  return data;
}

export interface EditAccountInput {
  id: string;
  email: string;
  userName: string;
  bio: string;
  birthdate: string;
  countryId: number;
  genderId: number;
  cityId: number;
  /** Kept images (with possibly changed isProfilePicture); omitted ones are soft-deleted. */
  images: ImageModel[];
  newImages: File[];
}

// Contract quirk kept from the old app: each existing image goes under
// "Images" as a JSON-stringified object; new files under "NewImages".
export async function editAccount(input: EditAccountInput): Promise<AccountDetails> {
  const form = new FormData();
  form.append("Id", input.id);
  form.append("Email", input.email);
  form.append("UserName", input.userName);
  form.append("Bio", input.bio);
  form.append("Birthdate", input.birthdate);
  form.append("CountryId", String(input.countryId));
  form.append("GenderId", String(input.genderId));
  form.append("CityId", String(input.cityId));

  for (const image of input.images) {
    form.append("Images", JSON.stringify(image));
  }
  for (const file of input.newImages) {
    form.append("NewImages", file);
  }

  const { data } = await http.put<AccountDetails>(`/profile/account/${input.id}`, form, {
    headers: { "Content-Type": "multipart/form-data" },
  });
  return data;
}
