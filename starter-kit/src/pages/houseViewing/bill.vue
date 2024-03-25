
<script  setup>
import useEmitter from '@/helper/useEmitter'
import { useBillStore } from '@/services/bill/useBillStore'
import { onMounted, ref, watch } from 'vue'
import ModalUpdateBill from './modules/modal-update-bill.vue'

const billStore = useBillStore()
const emitter = useEmitter()
const refModalUpdateBill = ref()
const showConfirmDialog = ref(false)
const billDeleteId = ref(0)

const bills = ref([])

const queryParams = ref({
})

const pagination = ref({
  pageSize: 10,
  pageNumber: 1,
  totalPages: 0,
})

const trangThaiBan = ref([
  { key: true, title: 'Đã bán' },
  { key: false, title: 'Chưa bán' },
])

watch(
  () => pagination.value.pageNumber,
  newVal => {
    getAllBill()
  },
)

watch(
  () => queryParams.value.keyword,
  newVal => {
    getAllBill()
  },
)

watch(
  () => queryParams.value.fromDate,
  newVal => {
    getAllBill()
  },
)

watch(
  () => queryParams.value.toDate,
  newVal => {
    getAllBill()
  },
)

watch(
  () => queryParams.value.isSold,
  newVal => {
    getAllBill()
  },
)


onMounted(() => {
  getAllBill()
})

const getAllBill = async () => {
  try {
    const params = { ...queryParams.value }

    if(params.pageSize !== 0) {
      params.pageSize = pagination.value.pageSize
      params.pageNumber = pagination.value.pageNumber
    }

    const res = (await billStore.getAllBill(params)).data

    bills.value = res.data

    pagination.value.totalPages = res.totalPages
    pagination.value.pageNumber = res.pageNumber
    pagination.value.pageSize = res.pageSize

  } catch (error) {

  }
}

const deleteBill = async isConfirm => {
  try {  
    if(isConfirm){
      await billStore.deleteBill(billDeleteId.value)
      getAllBill()
      emitter.emit('showAlert', {
        type: 'success',
        content: 'Success!',
      })
    }
  } catch (error) {
    emitter.emit('showAlert', {
      type: 'error',
      content: 'Server error!',
    })
  }
  showConfirmDialog.value = false
}

const openUpdateProductDialog = bill => {
  refModalUpdateBill.value.openDialog(bill)
}

// aaa
</script>

<template>
  <VCard style="padding: 50px">
    <VRow class="mb-5">
      <VCol cols="3">
        <VTextField
          v-model="queryParams.keyword"
          prepend-inner-icon="mdi-search"
          clearable
          label="Search"
        />
      </VCol>
      <VCol cols="2">
        <AppDateTimePicker
          v-model="queryParams.fromDate"
          clearable
          placeholder="Thời gian tạo từ"
        />
      </VCol>
      <VCol cols="2">
        <AppDateTimePicker
          v-model="queryParams.toDate"
          clearable
          placeholder="Thời gian tạo đến"
        />
      </VCol>
      <VCol cols="3">
        <VSelect
          v-model="queryParams.isSold"
          prepend-inner-icon="mdi-currency-usd"
          clearable
          label="Trạng thái bán"
          :items="trangThaiBan"
          item-value="key"
          item-text="title"
        />
      </VCol>
    </VRow>
    <div class="d-flex justify-end mb-3">
      <VBtn @click="openUpdateProductDialog(undefined)">
        Tạo phiếu xem nhà
      </VBtn>
    </div>
    <VTable fixed-header>
      <thead>
        <tr>
          <th class="text-left">
            #
          </th>
          <th class="text-left">
            Tên khách hàng
          </th>
          <th class="text-left">
            Số điện thoại khách hàng
          </th>
          <th class="text-left">
            Mô tả
          </th>
          <th
            class="text-left"
            style="width: 150px;"
          >
            Trạng thái bán
          </th>
          <th class="text-left">
            Thao tác
          </th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="(item, index) in bills"
          :key="index"
        >
          <td>{{ index + 1 }}</td>
          <td>{{ item.custumerName }}</td>
          <td>{{ item.custumerPhoneNumber }}</td>
          <td>{{ item.desciption }}</td>
          <td>
            <VSwitch
              v-model="item.banThanhCong"
              :color="item.banThanhCong ? 'success' : ''"
              readonly
              :label="item.banThanhCong ? 'Đã bán' : 'Có sẵn'"
            />
          </td>
          <td>
            <div style="display: flex;">
              <VBtn
                style="margin-right: 10px;"
                variant="text"
                size="small"
                @click="openUpdateProductDialog(item)"
              >
                Sửa
              </VBtn>
              <VBtn
                variant="text"
                size="small"
                color="#bc2f2f"
                @click="() => {
                  showConfirmDialog = true;
                  billDeleteId = item.id;
                }"
              >
                Xóa
              </VBtn>
            </div>
          </td>
        </tr>
      </tbody>
    </VTable>
    <VPagination
      v-if="pagination.totalPages > 0"
      v-model="pagination.pageNumber"
      :length="pagination.totalPages"
    />
    <ConfirmDialog
      :is-dialog-visible="showConfirmDialog"
      confirmation-question="Bạn có chắc chắn muốn xóa bản ghi này?"
      confirm-title="Success"
      @confirm="deleteBill"
    />
    <ModalUpdateBill
      ref="refModalUpdateBill"
      @refresh-data="getAllBill"
    />
  </VCard>
</template> 
