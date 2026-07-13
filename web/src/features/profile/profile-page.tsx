import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import dayjs from "dayjs";
import { useEffect, useRef, useState } from "react";
import { useForm } from "react-hook-form";
import { toast } from "sonner";
import { z } from "zod";
import { getCitiesByCountry, getCountries, getGenders } from "@/api/geo";
import { editAccount, getAccount } from "@/api/profile";
import type { AccountDetails, ImageModel } from "@/api/types";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
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
import { validation } from "@/lib/constants";
import { ageFrom, latestLegalBirthdate } from "@/lib/dates";
import { errorMessages } from "@/lib/errors";
import { useAuthStore } from "@/stores/auth";
import PhotoManager, { type NewPhoto } from "./photo-manager";

const schema = z.object({
  userName: z
    .string()
    .min(1, "Username is required")
    .max(validation.USERNAME_MAX_LENGTH, `Username must be at most ${validation.USERNAME_MAX_LENGTH} characters`),
  bio: z
    .string()
    .max(validation.BIO_MAX_LENGTH, `Bio must be at most ${validation.BIO_MAX_LENGTH} characters`),
  birthdate: z
    .string()
    .min(1, "Birthdate is required")
    .refine(
      (value) => ageFrom(value) >= validation.MINIMAL_AGE,
      `You must be at least ${validation.MINIMAL_AGE} years old`,
    ),
  genderId: z.string().refine((value) => Number(value) > 0, "Pick a gender"),
  countryId: z.string().refine((value) => Number(value) > 0, "Pick a country"),
  cityId: z.string().refine((value) => Number(value) > 0, "Pick a city"),
});

type FormValues = z.infer<typeof schema>;

function toFormValues(account: AccountDetails): FormValues {
  return {
    userName: account.userName,
    bio: account.bio ?? "",
    birthdate: dayjs(account.birthdate).format("YYYY-MM-DD"),
    genderId: account.genderId > 0 ? String(account.genderId) : "",
    countryId: account.countryId > 0 ? String(account.countryId) : "",
    cityId: account.cityId > 0 ? String(account.cityId) : "",
  };
}

export default function ProfilePage() {
  const myId = useAuthStore((s) => s.user?.id);

  const accountQuery = useQuery({
    queryKey: ["account", myId],
    queryFn: () => getAccount(myId!),
    enabled: !!myId,
  });

  if (accountQuery.isPending) return <ProfileSkeleton />;

  if (accountQuery.isError) {
    return (
      <div className="text-destructive mx-auto w-full max-w-5xl space-y-0.5 p-4 text-sm md:p-8">
        {errorMessages(accountQuery.error).map((line) => (
          <p key={line}>{line}</p>
        ))}
      </div>
    );
  }

  return <ProfileEditor account={accountQuery.data} />;
}

function ProfileEditor({ account }: { account: AccountDetails }) {
  const queryClient = useQueryClient();

  // Photo state: kept images (with editable isProfilePicture flags) + new files.
  const [keptImages, setKeptImages] = useState<ImageModel[]>(account.images);
  const [newPhotos, setNewPhotos] = useState<NewPhoto[]>([]);

  // Revoke outstanding preview URLs on unmount.
  const newPhotosRef = useRef(newPhotos);
  newPhotosRef.current = newPhotos;
  useEffect(
    () => () => {
      for (const photo of newPhotosRef.current) URL.revokeObjectURL(photo.url);
    },
    [],
  );

  const form = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: toFormValues(account),
  });

  const countryId = Number(form.watch("countryId"));
  const bioLength = form.watch("bio").length;

  const gendersQuery = useQuery({ queryKey: ["genders"], queryFn: getGenders });
  const countriesQuery = useQuery({ queryKey: ["countries"], queryFn: getCountries });
  const citiesQuery = useQuery({
    queryKey: ["cities", countryId],
    queryFn: () => getCitiesByCountry(countryId),
    enabled: countryId > 0,
  });

  // Placeholder entries with id 0 are skipped.
  const genders = (gendersQuery.data ?? []).filter((g) => g.id !== 0);
  const countries = (countriesQuery.data ?? []).filter((c) => c.countryId !== 0);
  const cities = (citiesQuery.data?.cities ?? []).filter((c) => c.cityId !== 0);

  const mutation = useMutation({
    mutationFn: (values: FormValues) =>
      editAccount({
        id: account.id,
        email: account.email,
        userName: values.userName,
        bio: values.bio,
        birthdate: values.birthdate,
        countryId: Number(values.countryId),
        genderId: Number(values.genderId),
        cityId: Number(values.cityId),
        images: keptImages,
        newImages: newPhotos.map((photo) => photo.file),
      }),
    onSuccess: (data) => {
      queryClient.setQueryData(["account", account.id], data);
      setKeptImages(data.images);
      for (const photo of newPhotos) URL.revokeObjectURL(photo.url);
      setNewPhotos([]);
      form.reset(toFormValues(data));
      toast.success("Profile updated");
    },
  });

  function makeProfilePicture(id: number) {
    setKeptImages((images) =>
      images.map((image) => ({ ...image, isProfilePicture: image.id === id })),
    );
  }

  function removeImage(id: number) {
    setKeptImages((images) => {
      const rest = images.filter((image) => image.id !== id);
      // Keep exactly one profile picture flag alive when images remain.
      if (rest.length > 0 && !rest.some((image) => image.isProfilePicture)) {
        return rest.map((image, index) => ({ ...image, isProfilePicture: index === 0 }));
      }
      return rest;
    });
  }

  function addFiles(files: FileList | null) {
    if (!files) return;
    const added = Array.from(files).map((file) => ({
      file,
      url: URL.createObjectURL(file),
    }));
    setNewPhotos((photos) => [...photos, ...added]);
  }

  function removeNewPhoto(url: string) {
    URL.revokeObjectURL(url);
    setNewPhotos((photos) => photos.filter((photo) => photo.url !== url));
  }

  function onSubmit(values: FormValues) {
    if (keptImages.length + newPhotos.length === 0) {
      toast.error("Keep or add at least one photo");
      return;
    }
    mutation.mutate(values);
  }

  return (
    <div className="mx-auto w-full max-w-5xl p-4 md:p-8">
      <h1 className="text-2xl font-bold tracking-tight">Your profile</h1>
      <p className="text-muted-foreground mt-1 text-sm">
        Keep your photos and details fresh — they are what others swipe on.
      </p>

      <Form {...form}>
        <form onSubmit={form.handleSubmit(onSubmit)}>
          <div className="mt-6 grid items-start gap-6 lg:grid-cols-5">
            {/* Photos */}
            <Card className="lg:col-span-2">
              <CardHeader>
                <CardTitle>Photos</CardTitle>
                <CardDescription>
                  Star a photo to make it your profile picture. Changes apply when you save.
                </CardDescription>
              </CardHeader>
              <CardContent>
                <PhotoManager
                  images={keptImages}
                  newPhotos={newPhotos}
                  onMakeProfilePicture={makeProfilePicture}
                  onRemoveImage={removeImage}
                  onAddFiles={addFiles}
                  onRemoveNewPhoto={removeNewPhoto}
                />
              </CardContent>
            </Card>

            {/* Details */}
            <Card className="lg:col-span-3">
              <CardHeader>
                <CardTitle>Details</CardTitle>
                <CardDescription>Tell the world who you are.</CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="space-y-2">
                  <Label htmlFor="profile-email">Email</Label>
                  <Input id="profile-email" value={account.email} readOnly disabled />
                </div>

                <FormField
                  control={form.control}
                  name="userName"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Username</FormLabel>
                      <FormControl>
                        <Input maxLength={validation.USERNAME_MAX_LENGTH} {...field} />
                      </FormControl>
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
                        <span className="text-muted-foreground text-xs tabular-nums">
                          {bioLength}/{validation.BIO_MAX_LENGTH}
                        </span>
                      </div>
                      <FormControl>
                        <Textarea
                          rows={3}
                          maxLength={validation.BIO_MAX_LENGTH}
                          placeholder="Say something about yourself…"
                          {...field}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <div className="grid gap-4 sm:grid-cols-2">
                  <FormField
                    control={form.control}
                    name="birthdate"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Birthdate</FormLabel>
                        <FormControl>
                          <Input type="date" max={latestLegalBirthdate()} {...field} />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  <FormField
                    control={form.control}
                    name="genderId"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Gender</FormLabel>
                        <FormControl>
                          <RadioGroup
                            className="flex flex-wrap gap-4 pt-1.5"
                            value={field.value}
                            onValueChange={field.onChange}
                          >
                            {genders.map((gender) => (
                              <div key={gender.id} className="flex items-center gap-2">
                                <RadioGroupItem
                                  id={`gender-${gender.id}`}
                                  value={String(gender.id)}
                                />
                                <Label htmlFor={`gender-${gender.id}`} className="font-normal">
                                  {gender.name}
                                </Label>
                              </div>
                            ))}
                          </RadioGroup>
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                </div>

                <div className="grid gap-4 sm:grid-cols-2">
                  <FormField
                    control={form.control}
                    name="countryId"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Country</FormLabel>
                        <Select
                          value={field.value}
                          onValueChange={(value) => {
                            field.onChange(value);
                            form.setValue("cityId", "");
                          }}
                        >
                          <FormControl>
                            <SelectTrigger className="w-full">
                              <SelectValue placeholder="Select a country" />
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
                          disabled={countryId <= 0 || citiesQuery.isLoading}
                        >
                          <FormControl>
                            <SelectTrigger className="w-full">
                              <SelectValue placeholder="Select a city" />
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
                </div>

                {mutation.isError && (
                  <div className="text-destructive space-y-0.5 text-sm">
                    {errorMessages(mutation.error).map((line) => (
                      <p key={line}>{line}</p>
                    ))}
                  </div>
                )}

                <Button type="submit" className="w-full sm:w-auto" disabled={mutation.isPending}>
                  {mutation.isPending ? "Saving…" : "Save changes"}
                </Button>
              </CardContent>
            </Card>
          </div>
        </form>
      </Form>
    </div>
  );
}

function ProfileSkeleton() {
  return (
    <div className="mx-auto w-full max-w-5xl p-4 md:p-8">
      <Skeleton className="h-8 w-44" />
      <Skeleton className="mt-2 h-4 w-72" />
      <div className="mt-6 grid items-start gap-6 lg:grid-cols-5">
        <Card className="lg:col-span-2">
          <CardContent className="pt-6">
            <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-2 xl:grid-cols-3">
              {Array.from({ length: 4 }, (_, index) => (
                <Skeleton key={index} className="aspect-square rounded-xl" />
              ))}
            </div>
          </CardContent>
        </Card>
        <Card className="lg:col-span-3">
          <CardContent className="space-y-4 pt-6">
            {Array.from({ length: 5 }, (_, index) => (
              <div key={index} className="space-y-2">
                <Skeleton className="h-4 w-24" />
                <Skeleton className="h-9 w-full" />
              </div>
            ))}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
