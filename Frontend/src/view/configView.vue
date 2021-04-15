<template>
	<brokers-view />
	<currencies-view class="mt-2" />
	<service-view class="mt-2" />
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import BrokersView from '@/view/brokersView.vue';
import CurrenciesView from '@/view/currenciesView.vue';
import ServiceView from '@/view/serviceView.vue';

@Options({
	name: 'ConfigView',
	components: {
		BrokersView,
		CurrenciesView,
		ServiceView,
	},
})
export default class ConfigView extends Vue {}
</script>
