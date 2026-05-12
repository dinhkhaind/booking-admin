// booking-shared.js
// Shared drawer and detail panel functions
// Depends on: openEditModal() defined in _BookingModal.cshtml inline script
// Must be loaded AFTER _BookingModal.cshtml partial is rendered

let currentDetailBookingId = null;

function openDetailDrawer() {
    document.getElementById('detailDrawer').classList.add('open');
    document.getElementById('drawerOverlay').classList.add('open');
}

function closeDetailDrawer() {
    document.getElementById('detailDrawer').classList.remove('open');
    document.getElementById('drawerOverlay').classList.remove('open');
}

function openDetailModal(bookingId) {
    currentDetailBookingId = bookingId;
    fetch(`/lich-phong/api/bookings/${bookingId}`)
        .then(r => r.json())
        .then(data => {
            const html = buildDetailHtml(data);
            document.getElementById('detailContent').innerHTML = html;
            openDetailDrawer();
        })
        .catch(err => alert('Error loading booking details: ' + err));
}

function buildDetailHtml(data) {
    let html = `
        <div class="booking-detail-panel">
            <!-- NHÓM 1: THÔNG TIN BOOKING -->
            <div class="detail-group">
                <h6 class="group-title">NHÓM 1 - THÔNG TIN BOOKING</h6>
                <div class="detail-row">
                    <div class="detail-label">Khách đại diện:</div>
                    <div class="detail-value">${data.customerName}</div>
                </div>
                <div class="detail-row">
                    <div class="detail-label">Đại lý:</div>
                    <div class="detail-value">${data.channelName}${data.channelTypeName ? ' <span class="badge bg-info">' + data.channelTypeName + '</span>' : ''}</div>
                </div>
                <div class="detail-row">
                    <div class="detail-label">Code đại lý:</div>
                    <div class="detail-value">${data.agencyBookingCode || '-'}</div>
                </div>
                <div class="detail-row">
                    <div class="detail-label">Code hệ thống:</div>
                    <div class="detail-value detail-readonly">${data.systemCode}</div>
                </div>
                <div class="detail-row">
                    <div class="detail-label">Tàu:</div>
                    <div class="detail-value">${data.boatName}</div>
                </div>
                <div class="detail-row">
                    <div class="detail-label">Phòng:</div>
                    <div class="detail-value">${data.roomCode}${data.roomTypeName ? ' (' + data.roomTypeName + ')' : ''}</div>
                </div>
                <div class="detail-row">
                    <div class="detail-label">Ngày check-in:</div>
                    <div class="detail-value">${data.checkIn}</div>
                </div>
                <div class="detail-row">
                    <div class="detail-label">Ngày check-out:</div>
                    <div class="detail-value">${data.checkOut}</div>
                </div>
                <div class="detail-row">
                    <div class="detail-label">Số đêm:</div>
                    <div class="detail-value">${data.nights} đêm</div>
                </div>
                <div class="detail-row">
                    <div class="detail-label">Gói lưu trú:</div>
                    <div class="detail-value">${data.packageName} (${data.packageCode})</div>
                </div>
                <div class="detail-row">
                    <div class="detail-label">Khách:</div>
                    <div class="detail-value">${data.adults} người lớn, ${data.children} trẻ em, ${data.infants} trẻ sơ sinh</div>
                </div>
                <div class="detail-row">
                    <div class="detail-label">Giá:</div>
                    <div class="detail-value detail-price" style="background-color: ${data.statusColor || '#f0f0f0'};">
                        ${data.totalPrice.toLocaleString('vi-VN')} ${data.currencyCode}
                        <span class="status-badge" style="background-color: ${data.statusColor}; color: white; padding: 2px 8px; border-radius: 3px; margin-left: 8px; font-size: 0.85rem;">
                            ${data.statusName}
                        </span>
                    </div>
                </div>
            </div>`;

    // NHÓM 2: DỊCH VỤ VẬN CHUYỂN (conditional)
    if (data.hasTransferService) {
        html += `
            <hr class="my-3">
            <div class="detail-group">
                <h6 class="group-title">NHÓM 2 - DỊCH VỤ VẬN CHUYỂN</h6>
                <div class="detail-row">
                    <div class="detail-label">Điểm đón:</div>
                    <div class="detail-value">${data.pickupPoint || '-'}</div>
                </div>
                <div class="detail-row">
                    <div class="detail-label">Điểm trả:</div>
                    <div class="detail-value">${data.dropoffPoint || '-'}</div>
                </div>
            </div>`;
    }

    // NHÓM 3: THÔNG TIN NỘI BỘ
    html += `
        <hr class="my-3">
        <div class="detail-group">
            <h6 class="group-title">NHÓM 3 - THÔNG TIN NỘI BỘ</h6>
            <div class="detail-row">
                <div class="detail-label">Nhân viên:</div>
                <div class="detail-value">${data.employeeName || '-'}</div>
            </div>
            <div class="detail-row">
                <div class="detail-label">Nhập bởi:</div>
                <div class="detail-value">${data.enteredByUsername || '-'}</div>
            </div>
            <div class="detail-row">
                <div class="detail-label">Ngày nhập:</div>
                <div class="detail-value">${data.entryDate || '-'}</div>
            </div>`;

    if (data.note) {
        html += `
            <div class="detail-row">
                <div class="detail-label">Ghi chú:</div>
                <div class="detail-value">${data.note}</div>
            </div>`;
    }

    html += `
        </div>
    </div>`;

    return html;
}

function editBooking() {
    if (!currentDetailBookingId) return;
    window.currentEditingBookingId = currentDetailBookingId;
    closeDetailDrawer();
    openEditModal(currentDetailBookingId);
}

function transferBooking() {
    alert('Chuyển tàu feature coming soon');
}

function printVoucher() {
    alert('In voucher feature coming soon');
}

async function cancelBooking() {
    if (!currentDetailBookingId) return;
    if (!confirm('Bạn có chắc chắn muốn huỷ booking này?')) return;

    try {
        const response = await fetch(`/lich-phong/api/bookings/${currentDetailBookingId}/cancel`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' }
        });

        if (response.ok) {
            alert('Booking đã được huỷ thành công');
            closeDetailDrawer();
            location.reload();
        } else {
            const data = await response.json();
            alert('Lỗi: ' + (data.error || 'Failed to cancel booking'));
        }
    } catch (error) {
        alert('Error: ' + error);
    }
}

// Wire up overlay click once DOM is ready
document.addEventListener('DOMContentLoaded', function () {
    const overlay = document.getElementById('drawerOverlay');
    if (overlay) overlay.addEventListener('click', closeDetailDrawer);
});
