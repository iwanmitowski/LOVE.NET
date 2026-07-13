import dayjs from "dayjs";
import relativeTime from "dayjs/plugin/relativeTime";

dayjs.extend(relativeTime);

export function ageFrom(birthdate: string): number {
  return dayjs().diff(dayjs(birthdate), "year");
}

/** Latest birthdate that still satisfies the 18+ rule. */
export function latestLegalBirthdate(): string {
  return dayjs().subtract(18, "year").format("YYYY-MM-DD");
}

/** Conversation-list style timestamp: time today, weekday this week, date otherwise. */
export function conversationTime(iso: string): string {
  const value = dayjs(iso);
  if (value.isSame(dayjs(), "day")) return value.format("HH:mm");
  if (value.isAfter(dayjs().subtract(7, "day"))) return value.format("ddd");
  return value.format("D MMM");
}

export function messageTime(iso: string): string {
  return dayjs(iso).format("HH:mm");
}

export function dayLabel(iso: string): string {
  const value = dayjs(iso);
  if (value.isSame(dayjs(), "day")) return "Today";
  if (value.isSame(dayjs().subtract(1, "day"), "day")) return "Yesterday";
  return value.format("D MMMM YYYY");
}

export function fromNow(iso: string): string {
  return dayjs(iso).fromNow();
}
