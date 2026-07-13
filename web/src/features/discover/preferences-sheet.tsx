import { useQuery } from "@tanstack/react-query";
import { SlidersHorizontal } from "lucide-react";
import { getGenders } from "@/api/geo";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import { Label } from "@/components/ui/label";
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group";
import { Separator } from "@/components/ui/separator";
import {
  Sheet,
  SheetContent,
  SheetDescription,
  SheetHeader,
  SheetTitle,
  SheetTrigger,
} from "@/components/ui/sheet";
import { Slider } from "@/components/ui/slider";
import { usePreferencesStore } from "@/stores/preferences";

/** Filter controls writing straight to the persisted preferences store (live filtering). */
export default function PreferencesSheet() {
  const { maxAge, maxDistance, aroundTheWorld, gender, set } = usePreferencesStore();
  const gendersQuery = useQuery({ queryKey: ["genders"], queryFn: getGenders });
  const genderOptions = [{ id: -1, name: "All" }, ...(gendersQuery.data ?? [])];

  return (
    <Sheet>
      <SheetTrigger asChild>
        <Button variant="outline" size="icon" aria-label="Preferences">
          <SlidersHorizontal />
        </Button>
      </SheetTrigger>
      <SheetContent side="right" className="overflow-y-auto">
        <SheetHeader>
          <SheetTitle>Preferences</SheetTitle>
          <SheetDescription>Filters apply to your deck instantly.</SheetDescription>
        </SheetHeader>

        <div className="space-y-6 px-4 pb-6">
          <section className="space-y-3">
            <div className="flex items-center justify-between">
              <Label htmlFor="max-age">Age</Label>
              <span className="text-muted-foreground text-sm">up to {maxAge}</span>
            </div>
            <Slider
              id="max-age"
              min={18}
              max={100}
              value={[maxAge]}
              onValueChange={([value]) => set({ maxAge: value })}
            />
          </section>

          <Separator />

          <section className="space-y-3">
            <Label>Distance</Label>
            <div className="flex items-center gap-2">
              <Checkbox
                id="around-the-world"
                checked={aroundTheWorld}
                onCheckedChange={(checked) => set({ aroundTheWorld: checked === true })}
              />
              <Label htmlFor="around-the-world" className="font-normal">
                All over the world
              </Label>
            </div>
            <div className="flex items-center justify-between">
              <span className="text-muted-foreground text-sm">Max distance</span>
              <span className="text-muted-foreground text-sm">{maxDistance} km</span>
            </div>
            <Slider
              min={10}
              max={600}
              step={10}
              disabled={aroundTheWorld}
              value={[maxDistance]}
              onValueChange={([value]) => set({ maxDistance: value })}
              aria-label="Max distance"
            />
          </section>

          <Separator />

          <section className="space-y-3">
            <Label>Interested in</Label>
            <RadioGroup
              value={String(gender)}
              onValueChange={(value) => set({ gender: Number(value) })}
            >
              {genderOptions.map((option) => (
                <div key={option.id} className="flex items-center gap-2">
                  <RadioGroupItem id={`gender-${option.id}`} value={String(option.id)} />
                  <Label htmlFor={`gender-${option.id}`} className="font-normal">
                    {option.name}
                  </Label>
                </div>
              ))}
            </RadioGroup>
          </section>
        </div>
      </SheetContent>
    </Sheet>
  );
}
