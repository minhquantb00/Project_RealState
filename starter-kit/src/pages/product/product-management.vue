<script setup>
import useEmitter from "@/helper/useEmitter"
import { useProductStore } from "@/services/product-services/useProductStore"
import moment from "moment"
import { onMounted, ref, watch } from "vue"
import ModalUpdateProduct from "./modules/modal-update-product.vue"

// #region data
const productStore = useProductStore()
const products = ref([])
const refUpdateProduct = ref()
const showConfirmDialog = ref(false)
const productDeleteId = ref()
const emitter = useEmitter()

const pagination = ref({
  pageSize: 10,
  pageNumber: 1,
  totalPages: 0,
})

const sapXepTheo = ref([
  { key: 'giaTangDan', title: 'Giá tăng dần' },
  { key: 'giaGiamDan', title: 'Giá giảm dần' },
  { key: 'bdsMoiNhat', title: 'BDS mới nhất' },
  { key: 'bdsCuNhat', title: 'BDS cũ nhất' },
])

const queryParams = ref({
  sortType: null,
})

watch(
  () => pagination.value.pageNumber,
  newVal => {
    getAllProduct()
  },
)

watch(
  () => queryParams.value.keyword,
  newVal => {
    getAllProduct()
  },
)

watch(
  () => queryParams.value.buildStart,
  newVal => {
    getAllProduct()
  },
)

watch(
  () => queryParams.value.buildEnd,
  newVal => {
    getAllProduct()
  },
)

watch(
  () => queryParams.value.minPrice,
  newVal => {
    getAllProduct()
  },
)

watch(
  () => queryParams.value.maxPrice,
  newVal => {
    getAllProduct()
  },
)

watch(
  () => queryParams.value.sortType,
  newVal => {
    getAllProduct()
  },
)

// #endregion

onMounted(() => {
  getAllProduct()
})

const getAllProduct = async () => {
  const params = { ...queryParams.value }

  if (params.pageSize !== 0) {
    params.pageSize = pagination.value.pageSize
    params.pageNumber = pagination.value.pageNumber
  }

  const res = (await productStore.getAll(params)).data

  products.value = res.data

  pagination.value.pageNumber = res.pageNumber
  pagination.value.totalPages = res.totalPages
  pagination.value.pageSize = res.pageSize
}

const deleteProduct = async isConfirm => {
  try {
    if (isConfirm) {
      await productStore.deleteProduct(productDeleteId.value)
      getAllProduct()
      emitter.emit("showAlert", {
        type: "success",
        content: "Success!",
      })
    }
  } catch (error) {
    emitter.emit("showAlert", {
      type: "error",
      content: "Server error!",
    })
  }
  showConfirmDialog.value = false
}

const openUpdateProductDialog = product => {
  refUpdateProduct.value.openDialog(product)
}
</script>

<template>
  <VCard style="padding: 50px">
    <VRow>
      <VCol cols="4">
        <VTextField
          v-model="queryParams.keyword"
          prepend-inner-icon="mdi-search"
          clearable
          label="Search"
        />
      </VCol>
      <VCol cols="2">
        <AppDateTimePicker
          v-model="queryParams.buildStart"
          clearable
          placeholder="Thời gian xây dựng từ"
        />
      </VCol>
      <VCol cols="2">
        <AppDateTimePicker
          v-model="queryParams.buildEnd"
          clearable
          placeholder="Thời gian xây dựng đến"
        />
      </VCol>
    </VRow>
    <div class="d-flex justify-end mb-3 mt-3">
      <VBtn @click="openUpdateProductDialog">
        Tạo mới sản phẩm
      </VBtn>
    </div>
    <VTable fixed-header>
      <thead>
        <tr>
          <th class="text-left">
            #
          </th>
          <th class="text-left">
            Họ tên chủ
          </th>
          <th class="text-left">
            Số điện thoại chủ
          </th>
          <th class="text-left">
            Thời gian xây dựng
          </th>
          <th class="text-left">
            Giấy chứng nhận đất đai 1
          </th>
          <th class="text-left">
            Giấy chứng nhận đất đai 2
          </th>
          <th class="text-left">
            Thao tác
          </th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="(item, index) in products"
          :key="index"
        >
          <td>{{ index + 1 }}</td>
          <td>{{ item.hostName }}</td>
          <td>{{ item.hostPhoneNumber }}</td>
          <td>{{ moment(item.build).format("DD/MM/YYYY") }}</td>
          <td>{{ item.certificateOfLand1 }}</td>
          <td>{{ item.certificateOfLand2 }}</td>
          <td>
            <div style="display: flex">
              <VBtn
                style="margin-right: 10px"
                variant="text"
                size="small"
                @click="openUpdateProductDialog(item)"
              >
                Cập nhật
              </VBtn>
              <VBtn
                variant="text"
                size="small"
                color="#bc2f2f"
                @click="
                  () => {
                    showConfirmDialog = true;
                    productDeleteId = item.id;
                  }
                "
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
      @confirm="deleteProduct"
    />
    <ModalUpdateProduct
      ref="refUpdateProduct"
      @refresh-data="getAllProduct"
    />
  </VCard>
</template>
