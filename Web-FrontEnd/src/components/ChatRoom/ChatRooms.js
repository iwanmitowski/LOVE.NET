import ChatRoomCard from "./ChatRoomCard";

export default function ChatRooms() {
    let rooms = [
        {
            id: "d4d3b3c1-edf4-4486-9cc2-f9ef11fc79d8",
            title: "Looking for love",
            imgSrc: "https://images.unsplash.com/photo-1500771967326-9b2f6200d1c6?ixlib=rb-4.0.3&ixid=MnwxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8&auto=format&fit=crop&w=1170&q=80",
            count: 0,
        },
        {
            id: "d9144113-bb62-4fd0-875c-a0eaf97c1aa7",
            title: "No plans for tonight",
            imgSrc: "https://images.unsplash.com/photo-1485872299829-c673f5194813?q=80&w=2054&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D=format&fit=crop&w=1170&q=80",
            count: 0,
        },
        {
            id: "10e4729f-d22f-4d89-9249-bb789f7251a5",
            title: "Sport",
            imgSrc: "https://images.unsplash.com/photo-1541252260730-0412e8e2108e?q=80&w=1948&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
        },
        {
            id: "836ea278-36e0-4b38-9253-09aa100f84a7",
            title: "Food",
            imgSrc: "https://images.unsplash.com/photo-1567620905732-2d1ec7ab7445?q=80&w=1980&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
            count: 0,
        },
        {
            id: "84db36d3-c876-4665-a089-a1af53c712be",
            title: "Looking for friends",
            imgSrc: "https://plus.unsplash.com/premium_photo-1665413642551-72f229a10f73?q=80&w=1925&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
            count: 0,
        },
        {
            id: "89bd0ac6-a882-4137-bb66-4d3ddef7399b",
            title: "Books",
            imgSrc: "https://plus.unsplash.com/premium_photo-1663040677874-db2bee655f48?q=80&w=2070&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
            count: 0,
        },
    ]

    return (
        <div className="d-flex flex-wrap justify-content-center">
            {rooms.map(r => <ChatRoomCard 
                id={r.id}
                title={r.title}
                imgSrc={r.imgSrc}
                count={r.count}
            />)}
        </div>
    )
}