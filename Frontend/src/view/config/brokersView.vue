<template>
	<div>
		<h2>Brokers</h2>
		<div class="row pl-3">
			<div v-for="broker in brokers" :key="broker.id">
				<broker-card :broker="broker" class="mr-2" />
			</div>
			<add-broker-card />
		</div>
	</div>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import BrokerCard from '@/component/config/brokerCard.vue';
import AddBrokerCard from '@/component/config/addBrokerCard.vue';
import { StateDto } from '@/api/state';
import { State } from 'vuex-class';

@Options({
	name: 'BrokersView',
	components: {
		BrokerCard,
		AddBrokerCard,
	},
})
export default class BrokersView extends Vue {
	@State('activeState')
	activeState!: StateDto;

	get brokers() {
		return this.activeState.brokers;
	}
}
</script>
