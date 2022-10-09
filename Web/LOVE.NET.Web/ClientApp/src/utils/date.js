export function getLatestLegal() {
  let latestLegalBirthdate = new Date();
  latestLegalBirthdate.setFullYear(latestLegalBirthdate.getFullYear() - 18);

  return latestLegalBirthdate;
}
