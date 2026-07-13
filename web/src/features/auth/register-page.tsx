import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery } from "@tanstack/react-query";
import { ArrowLeft, ArrowRight } from "lucide-react";
import { AnimatePresence, motion } from "motion/react";
import { useEffect, useRef, useState } from "react";
import { useForm, type FieldPath } from "react-hook-form";
import { Link, useNavigate } from "react-router";
import { z } from "zod";
import { getCitiesByCountry, getCountries, getGenders } from "@/api/geo";
import { register } from "@/api/identity";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Skeleton } from "@/components/ui/skeleton";
import { Textarea } from "@/components/ui/textarea";
import AuthCard from "@/features/auth/auth-card";
import PhotoPicker, { type PhotoItem } from "@/features/auth/photo-picker";
import StepIndicator from "@/features/auth/step-indicator";
import TermsDialog from "@/features/auth/terms-dialog";
import { validation } from "@/lib/constants";
import { ageFrom, latestLegalBirthdate } from "@/lib/dates";
import { errorMessages } from "@/lib/errors";
import { cn } from "@/lib/utils";

const schema = z
  .object({
    email: z.email("Invalid email"),
    userName: z
      .string()
      .min(1, "Username is required")
      .max(validation.USERNAME_MAX_LENGTH, "Username is too long"),
    password: z.string().min(validation.PASSWORD_MIN_LENGTH, "Password is too short"),
    confirmPassword: z.string().min(1, "Confirm your password"),
    birthdate: z
      .string()
      .min(1, "Birthdate is required")
      .refine(
        (value) => ageFrom(value) >= validation.MINIMAL_AGE,
        `You must be at least ${validation.MINIMAL_AGE} years old`,
      ),
    genderId: z.string().min(1, "Pick a gender"),
    countryId: z.string().refine((value) => Number(value) > 0, "Pick a country"),
    cityId: z.string().refine((value) => Number(value) > 0, "Pick a city"),
    bio: z
      .string()
      .min(1, "Tell us a bit about yourself")
      .max(validation.BIO_MAX_LENGTH, "Bio is too long"),
  })
  .refine((values) => values.password === values.confirmPassword, {
    message: "Passwords do not match",
    path: ["confirmPassword"],
  });

type FormValues = z.infer<typeof schema>;

const STEP_LABELS = ["Account", "About you", "Photos & finish"] as const;

const STEP_FIELDS: FieldPath<FormValues>[][] = [
  ["email", "userName", "password", "confirmPassword", "birthdate"],
  ["genderId", "countryId", "cityId", "bio"],
  [],
];

const stepVariants = {
  enter: (direction: number) => ({ opacity: 0, x: direction * 32 }),
  center: { opacity: 1, x: 0 },
  exit: (direction: number) => ({ opacity: 0, x: direction * -32 }),
};

export default function RegisterPage() {
  const navigate = useNavigate();
  const [step, setStep] = useState(0);
  const [direction, setDirection] = useState(1);
  const [photos, setPhotos] = useState<PhotoItem[]>([]);
  const [profileId, setProfileId] = useState<string | null>(null);
  const [agreed, setAgreed] = useState(false);

  const form = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      email: "",
      userName: "",
      password: "",
      confirmPassword: "",
      birthdate: "",
      genderId: "",
      countryId: "",
      cityId: "",
      bio: "",
    },
  });

  const gendersQuery = useQuery({ queryKey: ["genders"], queryFn: getGenders });
  const countriesQuery = useQuery({ queryKey: ["countries"], queryFn: getCountries });

  const selectedCountryId = Number(form.watch("countryId") || "0");
  const citiesQuery = useQuery({
    queryKey: ["cities", selectedCountryId],
    queryFn: () => getCitiesByCountry(selectedCountryId),
    enabled: selectedCountryId > 0,
  });

  const bioLength = form.watch("bio").length;

  // Falls back to the first photo so a lone upload is the profile picture.
  const effectiveProfileId = profileId ?? photos[0]?.id ?? null;

  // Revoke preview object URLs when the page unmounts.
  const photosRef = useRef(photos);
  photosRef.current = photos;
  useEffect(
    () => () => {
      for (const photo of photosRef.current) URL.revokeObjectURL(photo.url);
    },
    [],
  );

  const addPhotos = (files: File[]) => {
    setPhotos((previous) => {
      // The register API matches the profile picture by file name, so names must be unique.
      const existingNames = new Set(previous.map((photo) => photo.file.name));
      const added = files
        .filter((file) => !existingNames.has(file.name))
        .map((file) => ({
          id: crypto.randomUUID(),
          file,
          url: URL.createObjectURL(file),
        }));
      return [...previous, ...added];
    });
  };

  const removePhoto = (id: string) => {
    setPhotos((previous) => {
      const removed = previous.find((photo) => photo.id === id);
      if (removed) URL.revokeObjectURL(removed.url);
      return previous.filter((photo) => photo.id !== id);
    });
    setProfileId((previous) => (previous === id ? null : previous));
  };

  const mutation = useMutation({
    mutationFn: (values: FormValues) => {
      const profile = photos.find((photo) => photo.id === effectiveProfileId);
      return register({
        email: values.email,
        password: values.password,
        confirmPassword: values.confirmPassword,
        userName: values.userName,
        bio: values.bio,
        birthdate: values.birthdate,
        countryId: Number(values.countryId),
        genderId: Number(values.genderId),
        cityId: Number(values.cityId),
        image: profile?.file ?? null,
        newImages: photos.map((photo) => photo.file),
      });
    },
    onSuccess: (_data, values) => {
      void navigate(`/verify?email=${encodeURIComponent(values.email)}`);
    },
  });

  const isLastStep = step === STEP_LABELS.length - 1;

  const goNext = async () => {
    const valid = await form.trigger(STEP_FIELDS[step], { shouldFocus: true });
    if (!valid) return;
    setDirection(1);
    setStep((current) => current + 1);
  };

  const goBack = () => {
    setDirection(-1);
    setStep((current) => current - 1);
  };

  const genders = gendersQuery.data ?? [];
  // Index 0 of countries/cities is a "choose here" placeholder row (id 0).
  const countries = (countriesQuery.data ?? []).filter((country) => country.countryId !== 0);
  const cities = (citiesQuery.data?.cities ?? []).filter((city) => city.cityId !== 0);

  return (
    <AuthCard
      title="Create your account"
      subtitle="A few steps away from the sparks"
      widthClass="max-w-md"
    >
      <div className="space-y-5">
        <StepIndicator step={step} labels={STEP_LABELS} />

        <Form {...form}>
          <form
            className="space-y-4"
            onSubmit={(event) => {
              event.preventDefault();
              if (!isLastStep) {
                void goNext();
                return;
              }
              if (!agreed) return;
              void form.handleSubmit((values) => mutation.mutate(values))(event);
            }}
          >
            <div className="-mx-2 overflow-x-clip px-2">
              <AnimatePresence mode="wait" initial={false} custom={direction}>
                <motion.div
                  key={step}
                  custom={direction}
                  variants={stepVariants}
                  initial="enter"
                  animate="center"
                  exit="exit"
                  transition={{ duration: 0.2, ease: "easeOut" }}
                  className="space-y-4"
                >
                  {step === 0 && (
                    <>
                      <FormField
                        control={form.control}
                        name="email"
                        render={({ field }) => (
                          <FormItem>
                            <FormLabel>Email</FormLabel>
                            <FormControl>
                              <Input type="email" autoComplete="email" {...field} />
                            </FormControl>
                            <FormMessage />
                          </FormItem>
                        )}
                      />
                      <FormField
                        control={form.control}
                        name="userName"
                        render={({ field }) => (
                          <FormItem>
                            <FormLabel>Username</FormLabel>
                            <FormControl>
                              <Input autoComplete="username" {...field} />
                            </FormControl>
                            <FormMessage />
                          </FormItem>
                        )}
                      />
                      <div className="grid gap-4 sm:grid-cols-2">
                        <FormField
                          control={form.control}
                          name="password"
                          render={({ field }) => (
                            <FormItem>
                              <FormLabel>Password</FormLabel>
                              <FormControl>
                                <Input
                                  type="password"
                                  autoComplete="new-password"
                                  {...field}
                                />
                              </FormControl>
                              <FormMessage />
                            </FormItem>
                          )}
                        />
                        <FormField
                          control={form.control}
                          name="confirmPassword"
                          render={({ field }) => (
                            <FormItem>
                              <FormLabel>Confirm password</FormLabel>
                              <FormControl>
                                <Input
                                  type="password"
                                  autoComplete="new-password"
                                  {...field}
                                />
                              </FormControl>
                              <FormMessage />
                            </FormItem>
                          )}
                        />
                      </div>
                      <FormField
                        control={form.control}
                        name="birthdate"
                        render={({ field }) => (
                          <FormItem>
                            <FormLabel>Birthdate</FormLabel>
                            <FormControl>
                              <Input
                                type="date"
                                max={latestLegalBirthdate()}
                                autoComplete="bday"
                                {...field}
                              />
                            </FormControl>
                            <FormMessage />
                          </FormItem>
                        )}
                      />
                    </>
                  )}

                  {step === 1 && (
                    <>
                      <FormField
                        control={form.control}
                        name="genderId"
                        render={({ field }) => (
                          <FormItem>
                            <FormLabel>Gender</FormLabel>
                            {gendersQuery.isPending ? (
                              <Skeleton className="h-5 w-2/3" />
                            ) : (
                              <FormControl>
                                <RadioGroup
                                  className="flex flex-wrap gap-x-5 gap-y-2"
                                  value={field.value}
                                  onValueChange={field.onChange}
                                >
                                  {genders.map((gender) => (
                                    <div key={gender.id} className="flex items-center gap-2">
                                      <RadioGroupItem
                                        value={String(gender.id)}
                                        id={`gender-${gender.id}`}
                                      />
                                      <Label
                                        htmlFor={`gender-${gender.id}`}
                                        className="font-normal"
                                      >
                                        {gender.name}
                                      </Label>
                                    </div>
                                  ))}
                                </RadioGroup>
                              </FormControl>
                            )}
                            <FormMessage />
                          </FormItem>
                        )}
                      />
                      <FormField
                        control={form.control}
                        name="countryId"
                        render={({ field }) => (
                          <FormItem>
                            <FormLabel>Country</FormLabel>
                            {countriesQuery.isPending ? (
                              <Skeleton className="h-9 w-full" />
                            ) : (
                              <Select
                                value={field.value}
                                onValueChange={(value) => {
                                  field.onChange(value);
                                  form.setValue("cityId", "");
                                }}
                              >
                                <FormControl>
                                  <SelectTrigger className="w-full">
                                    <SelectValue placeholder="Choose your country" />
                                  </SelectTrigger>
                                </FormControl>
                                <SelectContent>
                                  {countries.map((country) => (
                                    <SelectItem
                                      key={country.countryId}
                                      value={String(country.countryId)}
                                    >
                                      {country.countryName}
                                    </SelectItem>
                                  ))}
                                </SelectContent>
                              </Select>
                            )}
                            <FormMessage />
                          </FormItem>
                        )}
                      />
                      <FormField
                        control={form.control}
                        name="cityId"
                        render={({ field }) => (
                          <FormItem>
                            <FormLabel>City</FormLabel>
                            <Select
                              value={field.value}
                              onValueChange={field.onChange}
                              disabled={selectedCountryId === 0 || citiesQuery.isLoading}
                            >
                              <FormControl>
                                <SelectTrigger className="w-full">
                                  <SelectValue
                                    placeholder={
                                      selectedCountryId === 0
                                        ? "Pick a country first"
                                        : citiesQuery.isLoading
                                          ? "Loading cities…"
                                          : "Choose your city"
                                    }
                                  />
                                </SelectTrigger>
                              </FormControl>
                              <SelectContent>
                                {cities.map((city) => (
                                  <SelectItem key={city.cityId} value={String(city.cityId)}>
                                    {city.cityName}
                                  </SelectItem>
                                ))}
                              </SelectContent>
                            </Select>
                            <FormMessage />
                          </FormItem>
                        )}
                      />
                      <FormField
                        control={form.control}
                        name="bio"
                        render={({ field }) => (
                          <FormItem>
                            <div className="flex items-center justify-between">
                              <FormLabel>Bio</FormLabel>
                              <span
                                className={cn(
                                  "text-xs tabular-nums",
                                  bioLength > validation.BIO_MAX_LENGTH
                                    ? "text-destructive"
                                    : "text-muted-foreground",
                                )}
                              >
                                {bioLength}/{validation.BIO_MAX_LENGTH}
                              </span>
                            </div>
                            <FormControl>
                              <Textarea
                                rows={3}
                                placeholder="What makes you, you?"
                                {...field}
                              />
                            </FormControl>
                            <FormMessage />
                          </FormItem>
                        )}
                      />
                    </>
                  )}

                  {step === 2 && (
                    <>
                      <PhotoPicker
                        photos={photos}
                        profileId={effectiveProfileId}
                        onAdd={addPhotos}
                        onRemove={removePhoto}
                        onChooseProfile={setProfileId}
                      />
                      <p className="text-muted-foreground text-xs">
                        Photos are optional — we will give you a default avatar if you skip
                        them.
                      </p>
                      <div className="flex items-center gap-2 text-sm">
                        <Checkbox
                          id="terms"
                          checked={agreed}
                          onCheckedChange={(checked) => setAgreed(checked === true)}
                        />
                        <Label htmlFor="terms" className="font-normal">
                          I agree to the
                        </Label>
                        <TermsDialog>
                          <button
                            type="button"
                            className="text-primary -ml-0.5 hover:underline"
                          >
                            Terms &amp; Conditions
                          </button>
                        </TermsDialog>
                      </div>
                    </>
                  )}
                </motion.div>
              </AnimatePresence>
            </div>

            {mutation.isError && (
              <div className="text-destructive space-y-0.5 text-sm">
                {errorMessages(mutation.error).map((line) => (
                  <p key={line}>{line}</p>
                ))}
              </div>
            )}

            <div className="flex gap-3 pt-1">
              {step > 0 && (
                <Button type="button" variant="outline" onClick={goBack}>
                  <ArrowLeft /> Back
                </Button>
              )}
              {isLastStep ? (
                <Button
                  type="submit"
                  className="flex-1"
                  disabled={!agreed || mutation.isPending}
                >
                  {mutation.isPending ? "Creating account…" : "Create account"}
                </Button>
              ) : (
                <Button type="button" className="flex-1" onClick={() => void goNext()}>
                  Continue <ArrowRight />
                </Button>
              )}
            </div>
          </form>
        </Form>

        <p className="text-muted-foreground text-center text-sm">
          Already have an account?{" "}
          <Link className="text-primary hover:underline" to="/login">
            Log in
          </Link>
        </p>
      </div>
    </AuthCard>
  );
}
