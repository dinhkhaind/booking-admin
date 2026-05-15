// booking-transfer.js
// Global transfer boat modal functions
// Reuses the same logic as the booking modal for loading room types and rooms
// Can be used from any page that includes the _TransferBoatModal partial

let transferModalData = { bookingId: null, currentBoat: null, currentRoom: null, bookingCode: null };

function openTransferModal(bookingId, bookingCode, boatName, roomCode) {
    transferModalData.bookingId = bookingId;
    transferModalData.bookingCode = bookingCode;
    transferModalData.boatName = boatName;
    transferModalData.roomCode = roomCode;

    document.getElementById('TransferBookingId').value = bookingId;
    document.getElementById('transferCurrentBookingCode').textContent = bookingCode;
    document.getElementById('transferCurrentBoatName').textContent = boatName;
    document.getElementById('transferCurrentRoomName').textContent = roomCode;

    // Reset form dropdowns
    document.getElementById('TransferBoatId').value = '';
    document.getElementById('TransferRoomTypeId').innerHTML = '<option value="">-- Chọn Loại Phòng --</option>';
    document.getElementById('TransferRoomId').innerHTML = '<option value="">-- Chọn Phòng --</option>';

    const modal = new bootstrap.Modal(document.getElementById('transferBoatModal'));
    modal.show();
}

// Load room types when boat changes (reused logic from booking modal)
async function loadTransferRoomTypes() {
    const boatId = document.getElementById('TransferBoatId').value;
    const roomTypeSelect = document.getElementById('TransferRoomTypeId');

    if (!boatId) {
        roomTypeSelect.innerHTML = '<option value="">-- Chọn Loại Phòng --</option>';
        document.getElementById('TransferRoomId').innerHTML = '<option value="">-- Chọn Phòng --</option>';
        return;
    }

    try {
        const response = await fetch(`/api/roomtypes?boatId=${boatId}`);
        const roomTypes = await response.json();

        roomTypeSelect.innerHTML = '<option value="">-- Chọn Loại Phòng --</option>';
        roomTypes.forEach(rt => {
            const option = document.createElement('option');
            option.value = rt.id;
            option.textContent = rt.name;
            roomTypeSelect.appendChild(option);
        });

        // Auto-select first room type and load rooms
        if (roomTypes.length > 0) {
            roomTypeSelect.value = roomTypes[0].id;
            await loadTransferRooms();
        } else {
            document.getElementById('TransferRoomId').innerHTML = '<option value="">-- Chọn Phòng --</option>';
        }
    } catch (error) {
        console.error('Error loading room types:', error);
        roomTypeSelect.innerHTML = '<option value="">-- Lỗi tải loại phòng --</option>';
    }
}

// Load rooms based on BoatId and RoomTypeId (reused logic from booking modal)
async function loadTransferRooms() {
    const boatId = document.getElementById('TransferBoatId').value;
    const roomTypeId = document.getElementById('TransferRoomTypeId').value;
    const roomSelect = document.getElementById('TransferRoomId');

    if (!boatId || !roomTypeId) {
        roomSelect.innerHTML = '<option value="">-- Chọn Phòng --</option>';
        return;
    }

    try {
        const url = `/api/rooms?boatId=${boatId}&roomTypeId=${roomTypeId}`;
        const response = await fetch(url);
        const rooms = await response.json();

        roomSelect.innerHTML = '<option value="">-- Chọn Phòng --</option>';
        rooms.forEach(room => {
            const option = document.createElement('option');
            option.value = room.id;
            option.textContent = `${room.roomCode} - ${room.roomName}`;
            roomSelect.appendChild(option);
        });

        // Auto-select first room
        if (rooms.length > 0) {
            roomSelect.value = rooms[0].id;
        }
    } catch (error) {
        console.error('Error loading rooms:', error);
        roomSelect.innerHTML = '<option value="">-- Lỗi tải phòng --</option>';
    }
}

async function saveTransferBoat() {
    const bookingId = document.getElementById('TransferBookingId').value;
    const newBoatId = document.getElementById('TransferBoatId').value;
    const newRoomId = document.getElementById('TransferRoomId').value;

    if (!bookingId || !newBoatId || !newRoomId) {
        alert('Vui lòng chọn tàu và phòng đích');
        return;
    }

    try {
        const response = await fetch('/api/bookings/transfer-boat', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                bookingId: parseInt(bookingId),
                newBoatId: parseInt(newBoatId),
                newRoomId: parseInt(newRoomId)
            })
        });

        const responseText = await response.text();
        let data = {};
        if (responseText) {
            try {
                data = JSON.parse(responseText);
            } catch (e) {
                console.error('JSON parse error:', e);
                alert('Lỗi: Server trả về dữ liệu không hợp lệ');
                return;
            }
        }

        if (data.success === true) {
            alert('Chuyển tàu thành công!');
            bootstrap.Modal.getInstance(document.getElementById('transferBoatModal')).hide();
            document.getElementById('transferBoatForm').reset();
            location.reload();
        } else {
            const errorMsg = data.error || 'Lỗi chuyển tàu';
            alert('Lỗi: ' + errorMsg);
        }
    } catch (error) {
        console.error('Error transferring boat:', error);
        alert('Lỗi: ' + error.message);
    }
}

// Initialize event listeners when modal is shown
function initTransferModalListeners() {
    document.getElementById('TransferBoatId').addEventListener('change', async function () {
        await loadTransferRoomTypes();
    });

    document.getElementById('TransferRoomTypeId').addEventListener('change', async function () {
        await loadTransferRooms();
    });
}

// Set up modal event listener
document.addEventListener('DOMContentLoaded', function() {
    const modalElement = document.getElementById('transferBoatModal');
    if (modalElement) {
        modalElement.addEventListener('show.bs.modal', function() {
            setTimeout(initTransferModalListeners, 100);
        });
    }
});

// Also initialize if modal already exists (for pages that load modals dynamically)
if (document.getElementById('transferBoatModal')) {
    document.getElementById('transferBoatModal').addEventListener('show.bs.modal', function() {
        setTimeout(initTransferModalListeners, 100);
    });
}
