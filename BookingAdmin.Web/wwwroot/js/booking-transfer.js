// booking-transfer.js
// Global transfer boat modal functions
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
    document.getElementById('TransferRoomId').innerHTML = '<option value="">-- Chọn Loại Phòng Trước --</option>';

    const modal = new bootstrap.Modal(document.getElementById('transferBoatModal'));
    modal.show();
}

function initTransferModalListeners() {
    // Load room types when boat changes
    const boatSelect = document.getElementById('TransferBoatId');
    if (boatSelect) {
        boatSelect.removeEventListener('change', loadTransferRoomTypes);
        boatSelect.addEventListener('change', loadTransferRoomTypes);

        // Auto-load room types if boat is already selected
        if (boatSelect.value) {
            loadTransferRoomTypes();
        }
    }

    // Load rooms when room type changes
    const roomTypeSelect = document.getElementById('TransferRoomTypeId');
    if (roomTypeSelect) {
        roomTypeSelect.removeEventListener('change', loadTransferRooms);
        roomTypeSelect.addEventListener('change', loadTransferRooms);

        // Auto-load rooms if room type is already selected
        if (roomTypeSelect.value && boatSelect && boatSelect.value) {
            loadTransferRooms();
        }
    }
}

async function loadTransferRoomTypes() {
    const boatId = document.getElementById('TransferBoatId').value;
    const roomTypeSelect = document.getElementById('TransferRoomTypeId');
    const roomSelect = document.getElementById('TransferRoomId');

    // Reset dependent dropdowns
    roomTypeSelect.innerHTML = '<option value="">-- Chọn Loại Phòng --</option>';
    roomSelect.innerHTML = '<option value="">-- Chọn Loại Phòng Trước --</option>';

    if (!boatId) return;

    try {
        const response = await fetch(`/api/room-types?boatId=${boatId}`);
        if (!response.ok) {
            console.error('Failed to load room types:', response.statusText);
            return;
        }

        const roomTypes = await response.json();
        if (!Array.isArray(roomTypes) || roomTypes.length === 0) {
            roomTypeSelect.innerHTML = '<option value="">-- Không có loại phòng --</option>';
            return;
        }

        roomTypes.forEach(rt => {
            const option = document.createElement('option');
            option.value = rt.id;
            option.textContent = rt.name;
            roomTypeSelect.appendChild(option);
        });
    } catch (error) {
        console.error('Error loading room types:', error);
        roomTypeSelect.innerHTML = '<option value="">-- Lỗi tải dữ liệu --</option>';
    }
}

async function loadTransferRooms() {
    const boatId = document.getElementById('TransferBoatId').value;
    const roomTypeId = document.getElementById('TransferRoomTypeId').value;
    const roomSelect = document.getElementById('TransferRoomId');

    // Reset room dropdown
    roomSelect.innerHTML = '<option value="">-- Chọn Phòng --</option>';

    if (!boatId || !roomTypeId) return;

    try {
        const response = await fetch(`/api/rooms?boatId=${boatId}&roomTypeId=${roomTypeId}`);
        if (!response.ok) {
            console.error('Failed to load rooms:', response.statusText);
            return;
        }

        const rooms = await response.json();
        if (!Array.isArray(rooms) || rooms.length === 0) {
            roomSelect.innerHTML = '<option value="">-- Không có phòng --</option>';
            return;
        }

        rooms.forEach(room => {
            const option = document.createElement('option');
            option.value = room.id;
            option.textContent = `${room.roomCode} - ${room.roomName}`;
            roomSelect.appendChild(option);
        });
    } catch (error) {
        console.error('Error loading rooms:', error);
        roomSelect.innerHTML = '<option value="">-- Lỗi tải dữ liệu --</option>';
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

// Initialize listeners when modal is shown
document.addEventListener('DOMContentLoaded', function() {
    const modalElement = document.getElementById('transferBoatModal');
    if (modalElement) {
        modalElement.addEventListener('show.bs.modal', function() {
            // Small delay to ensure DOM is ready
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
