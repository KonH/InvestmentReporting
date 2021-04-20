<template>
	<accounts-view />
	<brokers-view class="mt-2" />
	<service-view class="mt-2" />
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import AccountsView from '@/view/config/accountsView.vue';
import BrokersView from '@/view/config/brokersView.vue';
import ServiceView from '@/view/config/serviceView.vue';

@Options({
	name: 'ConfigView',
	components: {
		AccountsView,
		BrokersView,
		ServiceView,
	},
})
export default class ConfigView extends Vue {}
</script>
