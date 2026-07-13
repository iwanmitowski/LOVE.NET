// Typed mirrors of the microservices' REST/SignalR contracts (camelCase JSON).

export interface LoginResponse {
  id: string;
  token: string;
  email: string;
  userName: string;
  isAdmin: boolean;
  bio: string | null;
  birthdate: string;
  countryName: string | null;
  cityName: string | null;
  latitude: number;
  longitude: number;
  profilePicture: string | null;
}

export interface ImageModel {
  id: number;
  url: string;
  isProfilePicture: boolean;
}

/** Swipe card / match card (Matching service UserMatchModel). */
export interface UserCard {
  id: string;
  userName: string;
  bio: string | null;
  birthdate: string;
  age: number;
  images: ImageModel[];
  genderId: number;
  genderName: string | null;
  cityId: number;
  cityName: string | null;
  countryId: number;
  countryName: string | null;
  latitude: number;
  longitude: number;
  roomId: string | null;
}

export interface MatchesResponse {
  matches: UserCard[];
  totalMatches: number;
}

export interface LikeResponse {
  isMatch: boolean;
  user: UserCard | null;
}

/** Profile service account details (also the admin users row shape). */
export interface AccountDetails {
  id: string;
  email: string;
  userName: string;
  bio: string | null;
  birthdate: string;
  images: ImageModel[];
  genderId: number;
  genderName: string | null;
  cityId: number;
  cityName: string | null;
  countryId: number;
  countryName: string | null;
  latitude: number;
  longitude: number;
  isBanned: boolean;
}

export interface AdminUsersResponse {
  users: AccountDetails[];
  totalUsers: number;
}

export interface Statistics {
  usersCount: number;
  bannedUsersCount: number;
  matchesCount: number;
  likedUsersCount: number;
  notSwipedUsersCount: number;
  imagesCount: number;
  messagesCount: number;
}

export interface ChatMessage {
  id: string;
  roomId: string;
  userId: string;
  text: string | null;
  profilePicture: string | null;
  imageUrl: string | null;
  createdOn: string;
  /** Client-side only: optimistic bubble not yet acked by the hub. */
  pending?: boolean;
}

export interface ChatPage {
  messages: ChatMessage[];
  totalMessages: number;
}

export interface Chatroom {
  id: string;
  title: string;
  url: string;
}

export interface RoomSummary {
  roomId: string;
  lastMessageText: string | null;
  lastMessageImageUrl: string | null;
  lastMessageSenderId: string | null;
  lastMessageAt: string | null;
  unreadCount: number;
  peerLastReadAt: string | null;
}

export interface Country {
  countryId: number;
  countryName: string;
}

export interface City {
  cityId: number;
  cityName: string;
}

export interface CountryCities {
  countryId: number;
  countryName: string;
  cities: City[];
}

export interface Gender {
  id: number;
  name: string;
}

// ---- SignalR payloads (hub → client) ----

export interface UserInRoom {
  id: string;
  userName: string | null;
  profilePictureUrl: string | null;
}

export interface RefreshUsersList {
  users: UserInRoom[];
  hasLeft: boolean;
}

export interface MatchReceived {
  matchId: string;
  roomId: string;
  user: { id: string; userName: string; email: string; avatarUrl: string | null };
}

export interface MessageNotification {
  id: string;
  roomId: string;
  userId: string;
  senderUserName: string;
  text: string | null;
  imageUrl: string | null;
  createdOn: string;
}

export interface PeerTyping {
  roomId: string;
  userId: string;
  userName: string;
}

export interface ReadReceipt {
  roomId: string;
  userId: string;
  readAt: string;
}
