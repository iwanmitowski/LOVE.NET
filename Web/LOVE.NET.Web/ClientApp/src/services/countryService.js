import { globalConstants } from "../utils/constants";
import { instance } from "./api";

const baseUrl = globalConstants.API_URL + "countries";

export async function getAll() {
  try {
    const response = await instance.get(`${baseUrl}`);

    return response.data;
  } catch (error) {
    console.log(error.response.data.error);
    throw new Error(error.response.data.error);
  }
}

export async function getCitiesByCountryId(id) {
  try {
    const response = await instance.get(`${baseUrl}/${id}`);
    const data = await response.data;

    const cities = {
      countryId: data.countryId,
      cities: [{
        cityId: 0,
        cityName: "Choose city here",
      }, ...data.cities]
    };

    return cities;
  } catch (error) {
    console.log(error.response.data.error);
    throw new Error(error.response.data.error);
  }
}