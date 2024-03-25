
<script setup>
import AppDateTimePicker from '@/@core/components/app-form-elements/AppDateTimePicker.vue'
import ProductCardInfo from '@/pages/product-list-page/product-card-info.vue'
import ProductDetails from '@/pages/product-list-page/product-details.vue'
import { useProductStore } from "@/services/product-services/useProductStore"
import { onMounted, ref } from 'vue'

const pagination = ref({
  pageSize: 12,
  pageNumber: 1,
  totalPages: 0,
})

const sapXepTheo = ref([
  { key: 'giaTangDan', title: 'Giá tăng dần' },
  { key: 'giaGiamDan', title: 'Giá giảm dần' },
  { key: 'bdsMoiNhat', title: 'BDS mới nhất' },
  { key: 'bdsCuNhat', title: 'BDS cũ nhất' },
])

// #region data
const productStore = useProductStore()
const products = ref()
const productDetailsRef = ref()

const queryParams = ref({
  sortType: null,
})

// #endregion

// #region methods
onMounted(() => {
  getAllProduct()
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

const showDetails = product => {
  productDetailsRef.value.showDialog(product)
}

const getAllProduct = async () => {
  const params = { ...queryParams.value }
  if (params.pageSize !== 0) {
    params.pageSize = pagination.value.pageSize
    params.pageNumber = pagination.value.pageNumber
  }

  const res = (await productStore.getAll(params)).data

  products.value = res.data

  pagination.value.totalPages = res.totalPages
  pagination.value.pageNumber = res.pageNumber
  pagination.value.pageSize = res.pageSize

}

// #endregion
</script>

<template>
  <div>
    <VCard>
      <VCardText>
        <VRow>
          <VCol cols="2">
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
          <VCol cols="2">
            <VTextField
              v-model="queryParams.minPrice"
              type="number"
              min="0"
              prepend-inner-icon="mdi-money"
              label="Giá bán từ"
            />
          </VCol>
          <VCol cols="2">
            <VTextField
              v-model="queryParams.maxPrice"
              type="number"
              min="0"
              prepend-inner-icon="mdi-money"
              label="Giá bán đến"
            />
          </VCol>
          <VCol cols="2">
            <VSelect
              v-model="queryParams.sortType"
              prepend-inner-icon="mdi-filter-variant"
              clearable
              label="Select"
              :items="sapXepTheo"
              item-value="key"
              item-text="title"
            />
          </VCol>
        </VRow>
      </VCardText>
    </VCard>
    <VRow class="mt-5">
      <VCol
        v-for="product in products"
        :key="product.id"
        cols="12"
      >
        <ProductCardInfo
          :product-info="product"
          @showDetails="showDetails"
        />
      </VCol>
    </VRow>
    <VPagination
      v-if="pagination.totalPages > 0"
      v-model="pagination.pageNumber"
      :length="pagination.totalPages"
    />
    <ProductDetails ref="productDetailsRef" />
  </div>
</template>

