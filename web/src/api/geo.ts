import { http } from "@/api/http";
import type { Country, CountryCities, Gender } from "@/api/types";

export async function getCountries(): Promise<Country[]> {
  const { data } = await http.get<Country[]>("/countries");
  return data;
}

export async function getCitiesByCountry(countryId: number): Promise<CountryCities> {
  const { data } = await http.get<CountryCities>(`/countries/${countryId}`);
  return data;
}

export async function getGenders(): Promise<Gender[]> {
  const { data } = await http.get<Gender[]>("/genders");
  return data;
}
