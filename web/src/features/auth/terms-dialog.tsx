import type { ReactNode } from "react";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { ScrollArea } from "@/components/ui/scroll-area";

/** Condensed version of the old app's Terms & Conditions modal text. */
export default function TermsDialog({ children }: { children: ReactNode }) {
  return (
    <Dialog>
      <DialogTrigger asChild>{children}</DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Terms and conditions</DialogTitle>
          <DialogDescription>
            The short version of the rules for using LOVE.NET.
          </DialogDescription>
        </DialogHeader>
        <ScrollArea className="max-h-[50vh] pr-3">
          <div className="text-muted-foreground space-y-3 text-sm">
            <p>
              Welcome to LOVE.NET! By creating an account or continuing to use the
              site you accept these terms and conditions; if you do not agree with
              them, please do not use LOVE.NET.
            </p>
            <section className="space-y-1">
              <h3 className="text-foreground font-semibold">Cookies</h3>
              <p>
                We use cookies to keep parts of the site working (such as staying
                logged in), in line with our privacy policy.
              </p>
            </section>
            <section className="space-y-1">
              <h3 className="text-foreground font-semibold">License</h3>
              <p>
                LOVE.NET and its licensors own the material on the site. You may use
                it for personal purposes only — you must not republish, sell, rent,
                reproduce, or redistribute it.
              </p>
            </section>
            <section className="space-y-1">
              <h3 className="text-foreground font-semibold">Your content</h3>
              <p>
                You are responsible for anything you post. It must be yours to
                share, must not infringe anyone's rights, and must not be
                defamatory, offensive, unlawful, or used to promote business or
                unlawful activity. We may monitor and remove content that breaches
                these terms, and by posting you grant LOVE.NET a non-exclusive
                license to use, reproduce, and edit it.
              </p>
            </section>
            <section className="space-y-1">
              <h3 className="text-foreground font-semibold">Linking and framing</h3>
              <p>
                Approved organizations may link to our site as long as the link is
                not deceptive and does not falsely imply endorsement. You may not
                frame our pages or use our logo without written permission, and we
                may ask for links to be removed at any time.
              </p>
            </section>
            <section className="space-y-1">
              <h3 className="text-foreground font-semibold">Disclaimer</h3>
              <p>
                The service is provided free of charge and as-is. To the maximum
                extent permitted by law we exclude all warranties and are not liable
                for any loss or damage arising from its use — except for liability
                that cannot be excluded under applicable law. We do not guarantee
                the information on the site is complete, accurate, or always
                available.
              </p>
            </section>
          </div>
        </ScrollArea>
        <DialogFooter showCloseButton />
      </DialogContent>
    </Dialog>
  );
}
